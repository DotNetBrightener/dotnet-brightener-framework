using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DotNetBrightener.Integration.DataTablesServerProcessing.DataTablesExtensions;

namespace DotNetBrightener.Integration.DataTablesServerProcessing
{
	public class DataTableDataSourceFiltered<T> where T : class
	{
		public IEnumerable<T> Datasource { get; set; }
	}

	public interface IDataTableService
	{
		DataTablesResponse GetResponse<T, TOutput>(IQueryable<T> data, IDataTablesRequest request, Func<T, TOutput> expression = null) where T : class where TOutput : class;
	}

	public class DataTableService : IDataTableService
	{
		public DataTablesResponse GetResponse<T, TOutput>(IQueryable<T> data, IDataTablesRequest request, Func<T, TOutput> expression = null) where T : class where TOutput : class
		{
			var totalDataCount = data.Count();
			var myFilteredData = data.AsQueryable();

			if (request.Search != null && !string.IsNullOrEmpty(request.Search.Value))
			{
				var searchableColumns = request.Columns.Where(x => x.Searchable);
				myFilteredData = SearchData(myFilteredData, searchableColumns, request.Search);
			}

			var filteredColumns = request.Columns.GetFilteredColumns();
			foreach (var column in filteredColumns)
			{
				myFilteredData = FilterData(myFilteredData, column.Data, column.Search);
			}

			var sortedColumns = request.Columns.GetSortedColumns(request.Order);

			myFilteredData = SortData(myFilteredData, sortedColumns);

			var paged = myFilteredData.Skip(request.Start).Take(request.Length).ToArray();
			
			if (expression != null)
			{
				return new DataTablesResponse(request.Draw,
				                              paged.Select(expression),
				                              myFilteredData.Count(),
				                              totalDataCount);
			}

			return new DataTablesResponse(request.Draw,
										  paged,
										  myFilteredData.Count(),
										  totalDataCount);
		}

		private IQueryable<T> SearchData<T>(IQueryable<T> myFilteredData, IEnumerable<Column> searchableColumns, Search search)
		{
			var queryStrings = search.Value.Split(' ');

			foreach (var query in queryStrings)
			{
				var strBuilder = new List<String>();

				foreach (var col in searchableColumns)
				{
					strBuilder.Add(col.Data + " != NULL && " + col.Data + ".ToLower().Contains(@0)");
				}

				var orderQuery = string.Join(" || ", strBuilder);
				myFilteredData = myFilteredData.Where(orderQuery, query.ToLower());
			}

			return myFilteredData;
		}

		private static IQueryable<T> FilterData<T>(IQueryable<T> myFilteredData, string columnName, Search search)
		{
			return myFilteredData.Where(columnName + " != NULL && " + columnName + ".ToLower().Contains(@0)", search.Value.ToLower());
		}

		private static IQueryable<T> SortData<T>(IQueryable<T> myFilteredData, IEnumerable<Column> sortColumn)
		{
			var strBuilder = new List<string>();

			foreach (var col in sortColumn)
			{
				var sortDirection = col.SortDirection == Column.OrderDirection.Ascendant ? "asc" : "desc";
				strBuilder.Add(col.Data + " " + sortDirection);
			}

			var orderQuery = string.Join(",", strBuilder);

			if (string.IsNullOrEmpty(orderQuery))
				return myFilteredData;

			return myFilteredData.OrderBy(orderQuery);
		}
	}
}