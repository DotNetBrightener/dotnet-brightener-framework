
/****************************************************

 -----------------------------------------------------------------------
|          DotNet Brightener Auto CRUD Web API Generator Tool           |
|                               ---o0o---                               |
 -----------------------------------------------------------------------

This file is generated by an automation tool and it could be re-generated every time
you build the project.

Don't change this file as your changes will be lost when the file is re-generated.
If you need to change the content for this entity, update the DictionaryEntryDataService.cs file
which should be in the same folder as this file.

© 2024 DotNet Brightener. <admin@dotnetbrightener.com>

****************************************************/

using System;
using System.ComponentModel.DataAnnotations;

using DotNetBrightener;
using DotNetBrightener.DataAccess.Services;
using DotNetBrightener.LocaleManagement.Entities;

namespace DotNetBrightener.LocaleManagement.Data;

public partial interface IDictionaryEntryDataService : IBaseDataService<DictionaryEntry> { }

public partial class DictionaryEntryDataService : BaseDataService<DictionaryEntry>, IDictionaryEntryDataService {
    
    internal DictionaryEntryDataService(IRepository repository)
        : base(repository)
    {
    }

}