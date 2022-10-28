using DotNetBrightener.TemplateEngine.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetBrightener.TemplateEngine.Services
{
    public interface ITemplateRegistrationService 
    {
        Task RegisterTemplates();
    }

    public class TemplateRegistrationService : ITemplateRegistrationService, ITemplateStore
    {
        private readonly ITemplateContainer             _templateContainer;
        private readonly IEnumerable<ITemplateProvider> _providers;
        private readonly ITemplateRecordDataService     _repository;
        private readonly ILogger                        _logger;

        public TemplateRegistrationService(ITemplateContainer                   templateContainer,
                                           IEnumerable<ITemplateProvider>       providers,
                                           ITemplateRecordDataService           repository,
                                           ILogger<TemplateRegistrationService> logger)
        {
            _templateContainer = templateContainer;
            _providers         = providers;
            _repository        = repository;
            _logger            = logger;
        }

        public async Task RegisterTemplates()
        {
            if (!CheckCanProcessTemplate())
                return;

            foreach (var templateProvider in _providers)
            {
                var assemblyQualifiedName = templateProvider.GetType().Assembly.GetName().Name;

                _repository.UpdateMany(
                                       model => model.FromAssemblyName == assemblyQualifiedName,
                                       model => new TemplateRecord
                                       {
                                           IsDeleted = true
                                       });


                await templateProvider.RegisterTemplates(this);
            }
        }

        private bool CheckCanProcessTemplate()
        {
            try
            {
                _repository.Fetch().Count();

                return true;
            }
            catch (InvalidOperationException exception)
            {
                _logger.LogWarning(exception, "Error occurs while checking for template record table");

                if (exception.Message.Contains($"Cannot create a DbSet for '{nameof(TemplateRecord)}'"))
                {
                    return false;
                }

                throw;
            }
        }

        async Task ITemplateStore.RegisterTemplate<TTemplate>()
        {
            if (!CheckCanProcessTemplate())
                return;

            _templateContainer.RegisterTemplate<TTemplate>();

            var type = typeof(TTemplate).FullName;

            var templateFields = GetTemplateFields<TTemplate>();
            var assemblyName   = typeof(TTemplate).Assembly.GetName().Name;

            _logger.LogInformation($"Registering template type {type}");

            var templatesToUpdate = _repository.Fetch(x => x.TemplateType == type);

            if (!templatesToUpdate.Any())
            {
                _logger.LogInformation($"No template type {type} registered. Inserting new record...");
                var templateRecord = new TemplateRecord
                {
                    TemplateType     = type,
                    TemplateContent  = "",
                    TemplateTitle    = "",
                    CreatedDate      = DateTimeOffset.Now,
                    ModifiedDate     = DateTimeOffset.Now,
                    Fields           = templateFields,
                    FromAssemblyName = assemblyName
                };

                await _repository.InsertAsync(templateRecord);
            }
            else
            {
                foreach (var templateRecord in templatesToUpdate)
                {
                    templateRecord.IsDeleted        = false;
                    templateRecord.Fields           = templateFields;
                    templateRecord.FromAssemblyName = assemblyName;

                }

                _repository.Update(templatesToUpdate);
            }
        }

        private List<string> GetTemplateFields<TTemplate>() where TTemplate : ITemplateModel
        {
            var type = typeof(TTemplate);

            var fieldNamesFromType = GetFieldNamesFromType(type);

            return fieldNamesFromType;
        }

        private List<string> GetFieldNamesFromType(Type         type,
                                                   string       name          = "",
                                                   List<string> recursiveList = null,
                                                   int          level         = 0)
        {
            if (recursiveList == null)
                recursiveList = new List<string>();

            if (level > 3)
                return recursiveList;

            var properties = type.GetProperties();

            foreach (var property in properties)
            {
                recursiveList.Add($"{name}.{property.Name}".Trim('.'));

                if (property.PropertyType.IsClass &&
                    property.PropertyType.IsNotSystemType())
                {
                    GetFieldNamesFromType(property.PropertyType,
                                          $"{name}.{property.Name}".Trim('.'),
                                          recursiveList,
                                          level + 1);
                }
            }

            return recursiveList;
        }
    }
}