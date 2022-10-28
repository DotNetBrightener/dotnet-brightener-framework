﻿using DotNetBrightener.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DotNetBrightener.TemplateEngine.Entity
{
    public class TemplateRecord : BaseEntityWithAuditInfo
    {
        public string TemplateType { get; set; }

        public string TemplateTitle { get; set; }

        public string TemplateContent { get; set; }

        [NotMapped]
        public List<string> Fields
        {
            get =>
                FieldsString?.Split(new[]
                                    {
                                        ';'
                                    },
                                    StringSplitOptions.RemoveEmptyEntries)
                             .ToList() ?? new List<string>();
            set => FieldsString = string.Join(";", value);
        }

        [Column("Fields")]
        public string FieldsString { get; set; }

        public string FromAssemblyName { get; set; }
    }
}