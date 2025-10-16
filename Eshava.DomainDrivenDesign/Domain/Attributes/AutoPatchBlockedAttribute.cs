using System;

namespace Eshava.DomainDrivenDesign.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoPatchBlockedAttribute : Attribute
    {

    }
}