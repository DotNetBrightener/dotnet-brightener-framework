using DotNetBrightener.TemplateEngine.Attributes;
using DotNetBrightener.TemplateEngine.Exceptions;
using DotNetBrightener.TemplateEngine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotNetBrightener.TemplateEngine.Services
{
    public interface ITemplateContainer
    {
        void RegisterTemplate<TTemplateType>();

        List<Type> GetAllTemplateTypes();

        Type GetTemplateTypeByName(string templateTypeName);

        TemplateListItemModel GetTemplateInformation(string templateTypeName);
    }

    public class TemplateContainer : ITemplateContainer
    {
        private readonly ConcurrentDictionary<string, Type> _templateTypesList = new ConcurrentDictionary<string, Type>();

        public void RegisterTemplate<TTemplateType>()
        {
            var typeName = typeof(TTemplateType).FullName;
            if (typeName == null)
                return;

            _templateTypesList.TryAdd(typeName, typeof(TTemplateType));
        }

        public List<Type> GetAllTemplateTypes()
        {
            return _templateTypesList.Values.ToList();
        }

        public Type GetTemplateTypeByName(string templateTypeName)
        {
            if (_templateTypesList.TryGetValue(templateTypeName, out var templateType))
                return templateType;

            return null;
        }

        public TemplateListItemModel GetTemplateInformation(string templateTypeName)
        {
            var templateType = GetTemplateTypeByName(templateTypeName);

            if (templateType == null)
                throw new TemplateTypeNotFoundException(templateTypeName);
            
            var templateInformation = new TemplateListItemModel
                                      {
                                          TemplateName = GetFormattedTemplateName(templateType.Name),
                                          TemplateType = templateTypeName
                                      };

            var templateDescriptionAttribute = templateType.GetCustomAttribute<TemplateDescriptionAttribute>();
            if (templateDescriptionAttribute != null)
            {
                templateInformation.TemplateDescription = templateDescriptionAttribute.TemplateDescription;
                templateInformation.TemplateDescriptionKey = templateDescriptionAttribute.TemplateDescriptionKey;
            }

            return templateInformation;
        }

        private string GetFormattedTemplateName(string templateTypeName)
        {
            return templateTypeName.CamelFriendly();
        }
    }
}