using System.Security.Cryptography;
using System.Text;

namespace API.Utils;

public class Cryptics
{
    public static string ComputeHash(object itemDto)
    {
        var propertiesToInclude = new[]
        {
            "Barcode",
            "Brand",
            "Generic",
            "Classification",
            "Formulation",
            "Location",
            "Wholesale",
            "Retail",
            "Stock",
            "LowThreshold",
            "Company",
            "HasExpiry",
            "Expiry",
            "IsReagent",
            "UsesMax",
            "UsesLeft",
            "IsExpired",
            "IsDeleted"
        };

        var propertyValues = itemDto
            .GetType()
            .GetProperties()
            .Where(p => propertiesToInclude.Contains(p.Name))
            .Select(p => p.GetValue(itemDto)?.ToString() ?? string.Empty);

        var input = string.Join("-", propertyValues);
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        var finalHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        Console.WriteLine(finalHash);
        return finalHash;
    }
}