using System;
using Microsoft.AspNetCore.Authorization;

namespace GSA.UnliquidatedObligations.Web
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for all permission based authorization attributes. Uses Enum to 
    /// create the policy name for AuthorizeAttribute.
    /// 
    /// It cannot be a generic attribute. https://stackoverflow.com/questions/294216/why-does-c-sharp-forbid-generic-attribute-types
    /// so the class uses types and reflection.
    /// </summary>
    public abstract class PermissionAuthorizeAttributeBase : AuthorizeAttribute
    {
        /// <inheritdoc />
        /// <summary>
        /// Constructor. Provides AuthorizeAttribute with a policy name.
        /// </summary>
        /// <param name="policyNamePrefix">Prefix to the policy name (e.g. derived class's name)</param>
        /// <param name="enumType">Enum type (e.g. TenantPermissions)</param>
        /// <param name="enumValue">Enum value (e.g. TenantPermissions.UserCreate)</param>
        protected PermissionAuthorizeAttributeBase(string policyNamePrefix, Type enumType, object enumValue)
            : base(CreatePolicyName(policyNamePrefix, enumType, enumValue))
        {

        }

        //todo: This method is expected to return the same result as PortalPermissionHelpers.CreatePolicyName. 
        protected static string CreatePolicyName(string policyNamePrefix, Type enumType, object enumValue)
        {
            if (!enumType.IsEnum)
            {
                throw new Exception($"Type {enumType} must be an enum for {nameof(PermissionAuthorizeAttributeBase)} attribute");
            }
            if (!Enum.TryParse(enumType, enumValue.ToString(), true, out var e))
            {
                throw new Exception($"Object {enumValue} must be of Type {enumType} for {nameof(PermissionAuthorizeAttributeBase)} attribute");
            }
            policyNamePrefix = policyNamePrefix ?? nameof(PermissionAuthorizeAttributeBase);
            var result = $"{policyNamePrefix}.{e}";
            return result;
        }
    }
}
