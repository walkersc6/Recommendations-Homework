using CsvHelper.Configuration;
using Recommendations.Models;
namespace Recommendations.Models
{
    public sealed class ContentMap : ClassMap<Content>
    {
        public ContentMap()
        {
            Map(m => m.contentId).TypeConverterOption.Format("G");
            Map(m => m.Top1).TypeConverterOption.Format("G");
            Map(m => m.Top2).TypeConverterOption.Format("G");
            Map(m => m.Top3).TypeConverterOption.Format("G");
            Map(m => m.Top4).TypeConverterOption.Format("G");
            Map(m => m.Top5).TypeConverterOption.Format("G");
        }
    }
}
