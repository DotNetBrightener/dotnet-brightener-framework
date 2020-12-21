using System.Collections.Generic;
using System.Linq;

namespace WebEdFramework.Modular.Mvc
{
    internal class PageRenderResource
    {
        private int _currentPageLevel = 0;

        private readonly Dictionary<long, List<string>> _requireScriptUrls = new Dictionary<long, List<string>>();
        private readonly Dictionary<long, List<string>> _scriptUrls = new Dictionary<long, List<string>>();
        private readonly Dictionary<long, List<string>> _inlineScripts = new Dictionary<long, List<string>>();
        private readonly Dictionary<long, List<string>> _styleUrls = new Dictionary<long, List<string>>();

        public PageRenderResource()
        {
            InitializePageLevelResources();
        }

        public void NewPageLevel()
        {
            _currentPageLevel++;
            InitializePageLevelResources();
        }

        public void AddRequireScript(string scriptUrl)
        {
            _requireScriptUrls[_currentPageLevel].Add(scriptUrl);
        }

        public void AddScriptUrl(string scriptUrl)
        {
            _scriptUrls[_currentPageLevel].Add(scriptUrl);
        }

        public void AddScript(string scriptContent)
        {
            _inlineScripts[_currentPageLevel].Add(scriptContent);
        }

        public void AddStyleUrl(string styleUrl)
        {
            _styleUrls[_currentPageLevel].Add(styleUrl);
        }

        public List<string> GetOrderedScriptUrls()
        {
            return _requireScriptUrls.Values
                                     .Reverse()
                                     .SelectMany(_ => _)
                                     .Concat(_scriptUrls.Values
                                                        .Reverse()
                                                        .SelectMany(_ => _))
                                     .Distinct()
                                     .ToList();
        }

        public List<string> GetOrderedScriptContents()
        {
            return _inlineScripts.Values
                                 .Reverse()
                                 .SelectMany(_ => _)
                                 .ToList();
        }

        public List<string> GetOrderedStyleUrls()
        {
            return _styleUrls.Values
                             .Reverse()
                             .SelectMany(_ => _)
                             .Distinct()
                             .ToList();
        }

        private void InitializePageLevelResources()
        {
            _requireScriptUrls.Add(_currentPageLevel, new List<string>());
            _scriptUrls.Add(_currentPageLevel, new List<string>());
            _inlineScripts.Add(_currentPageLevel, new List<string>());
            _styleUrls.Add(_currentPageLevel, new List<string>());
        }
    }
}
