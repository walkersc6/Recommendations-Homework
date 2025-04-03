using CsvHelper.Configuration;
using Recommendations.Models;
namespace Recommendations.Models 
{ 
    public sealed class CollabMap : ClassMap<Collab>
    {
        public CollabMap()
        {
            Map(m => m.contentId).TypeConverterOption.Format("G");
            Map(m => m.IfYouRead).TypeConverterOption.Format("G");
            Map(m => m.Recommendation1).TypeConverterOption.Format("G");
            Map(m => m.Recommendation2).TypeConverterOption.Format("G");
            Map(m => m.Recommendation3).TypeConverterOption.Format("G");
            Map(m => m.Recommendation4).TypeConverterOption.Format("G");
            Map(m => m.Recommendation5).TypeConverterOption.Format("G");
        }
    }
}
