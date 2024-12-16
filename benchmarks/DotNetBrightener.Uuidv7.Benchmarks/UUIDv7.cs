using System.Security.Cryptography;

namespace DotNetBrightener.Uuidv7.Benchmarks;

public class Uuid7
{
    private static long TimeNs()
    {
        return 100 * (DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks);
    }

    private static long _x   = 0;
    private static long _y   = 0;
    private static long _z   = 0;
    private static int  _seq = 0;

    private static long _x_asOf   = 0;
    private static long _y_asOf   = 0;
    private static long _z_asOf   = 0;
    private static int  _seq_asOf = 0;

    public static Guid Guid(long? asOfNs = null)
    {
        int uuidVersion = 7;

        int uuidVariant = 0b10;
        int maxSeqValue = 0x3FFF;

        long ns;
        if (asOfNs == null)
            ns = TimeNs();
        else if (asOfNs == 0)
            return new Guid("00000000-0000-0000-0000-000000000000");
        else
            ns = (long)asOfNs;

        // Get timestamp components of length 32, 16, 12 bits,
        // with the first 36 bits being whole seconds and
        // the remaining 24 bits being fractional seconds.
        long x = Math.DivRem(ns, 16_000_000_000L, out long rest1);
        long y = Math.DivRem(rest1 << 16, 16_000_000_000L, out long rest2);
        long z = Math.DivRem(rest2 << 12, 16_000_000_000L, out long _);

        int seq;
        if (asOfNs != null)
        {
            if (x == _x && y == _y && z == _z)
            {
                // Shouldn't be possible to call often enough that seq overflows
                // before the next time tick. If that does happen
                // subsequent uuids with that time tick will be unique
                // (because of the random bytes) but no longer ordered.
                if (_seq < maxSeqValue)
                    _seq += 1;
            }
            else
            {
                _seq = 0;
                _x   = x;
                _y   = y;
                _z   = z;
            }
            seq = _seq;
        }
        else
        {
            // Check other counters if using asOfNs
            if (x == _x_asOf && y == _y_asOf && z == _z_asOf)
            {
                if (_seq_asOf < maxSeqValue)
                    _seq_asOf += 1;
            }
            else
            {
                _seq_asOf = 0;
                _x_asOf   = x;
                _y_asOf   = y;
                _z_asOf   = z;
            }
            seq = _seq_asOf;
        }

        // Last 8 bytes of uuid have variant and sequence in first two bytes,
        // then six bytes of randomness.
        var last8Bytes = new byte[8];
        RandomNumberGenerator.Fill(last8Bytes);
        last8Bytes[0] = (byte)(uuidVariant << 6 | seq >> 8);
        last8Bytes[1] = (byte)(seq & 0xFF);

        return new Guid(
                        (int)x,
                        (short)y,
                        (short)((uuidVersion << 12) + z & 0xFFFF),
                        last8Bytes
                       );
    }
}