using System;

namespace TEngine
{
    #region Attribute

    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FixedUpdateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class LateUpdateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleLoginAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RoleLogoutAttribute : Attribute
    {
    }

    #endregion
}