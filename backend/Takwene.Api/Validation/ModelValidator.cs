using System.ComponentModel.DataAnnotations;
using Takwene.Api.Services;
namespace Takwene.Api.Validation;
public static class ModelValidator
{
    public static void Validate(object model)
    {
        var errors = new List<ValidationResult>();
        if (Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties:true)) return;
        var fields = errors.SelectMany(error => error.MemberNames.DefaultIfEmpty("input").Select(member => (member, message:error.ErrorMessage ?? "Invalid value.")))
            .GroupBy(item => item.member).ToDictionary(group => group.Key, group => group.Select(item => item.message).Distinct().ToArray());
        throw new ApiException(400, "One or more validation errors occurred.", fields);
    }
}
