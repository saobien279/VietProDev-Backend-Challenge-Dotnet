using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Units
{
    public class UpdateUnitRequest : INormalizable
    {
        // Id is NOT in body — comes from route
        public string UnitName { get; set; } = null!;

        public void Normalize()
        {
            UnitName = string.IsNullOrWhiteSpace(UnitName)
                ? string.Empty
                : Regex.Replace(UnitName.Trim(), @"\s+", " ");
        }
    }
}
