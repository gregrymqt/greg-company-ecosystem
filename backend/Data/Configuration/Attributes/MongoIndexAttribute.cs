using System;

namespace MeuCrudCsharp.Data.Configuration.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class MongoIndexAttribute : Attribute
{
    public bool Descending { get; set; }
    public bool Unique { get; set; }
}
