using System.ComponentModel;

namespace PS5CodeReader
{
    internal static class Extensions
    {
        internal static void EnumForComboBox<EnumType>(this ComboBox comboBox) => comboBox.DataSource = Enum.GetValues(typeof(EnumType))
          .Cast<Enum>()
          .Select(Value =>
              {
                  var Description = string.Empty;
                  var fieldInfo = Value.GetType().GetField(Value.ToString());
                  if (fieldInfo != null)
                  {
                      var attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
                      Description = attribute?.Description;
                  }
                  return new
                  {
                      Description,
                      Value
                  };
              }
          )
          .OrderBy(item => item.Value)
          .ToList();
    }
}
