using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace BlazorApp1
{
    public class ImportService
    {
        private const int SRID_WGS84 = 4326;

        private static readonly FileStreamOptions ReadWriteFileStreamOptions = new()
        {
            Mode = FileMode.OpenOrCreate,
            Access = FileAccess.ReadWrite,
            Share = FileShare.ReadWrite,
            Options = FileOptions.Asynchronous
        };

        private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(PrecisionModels.Floating), SRID_WGS84);

        public static async Task<IEnumerable<StateWithGeometry>> GetStatesWithGeometry(string filePath)
        {
            using var streamReader = new StreamReader(filePath, ReadWriteFileStreamOptions);
            var json = await streamReader.ReadToEndAsync();
            var jsonSerializerSettings = new JsonSerializerSettings();
            var geoJsonReader = new GeoJsonReader(GeometryFactory, jsonSerializerSettings);
            var features = geoJsonReader.Read<FeatureCollection>(json);
            var states = new List<StateWithGeometry>();
            foreach (var feature in features)
            {
                var name = feature.Attributes
                    .GetOptionalValue("Name")
                        ?.ToString();

                var abbreviation = feature.Attributes
                    .GetOptionalValue("State")
                    ?.ToString();

                var fipsCode = int.TryParse(feature.Attributes
                    .GetOptionalValue("FIPS")
                    ?.ToString(), out var i)
                        ? i
                        : 0;

                var border = feature?.Geometry;

                var state = new StateWithGeometry()
                {
                    Name = name!,
                    Abbreviation = abbreviation!,
                    FipsCode = fipsCode!,
                    Border = border!
                };

                states.Add(state);
            }
            return states;
        }
        public static async Task<IEnumerable<StateWithoutGeometry>> GetStatesWithoutGeometry(string filePath)
        {
            using var streamReader = new StreamReader(filePath, ReadWriteFileStreamOptions);
            var json = await streamReader.ReadToEndAsync();
            var jsonSerializerSettings = new JsonSerializerSettings();
            var geoJsonReader = new GeoJsonReader(GeometryFactory, jsonSerializerSettings);
            var features = geoJsonReader.Read<FeatureCollection>(json);
            var states = new List<StateWithoutGeometry>();
            foreach (var feature in features)
            {
                var name = feature.Attributes
                    .GetOptionalValue("Name")
                        ?.ToString();

                var abbreviation = feature.Attributes
                    .GetOptionalValue("State")
                    ?.ToString();

                var fipsCode = int.TryParse(feature.Attributes
                    .GetOptionalValue("FIPS")
                    ?.ToString(), out var i)
                        ? i
                        : 0;

                var state = new StateWithoutGeometry()
                {
                    Name = name!,
                    Abbreviation = abbreviation!,
                    FipsCode = fipsCode!,
                };

                states.Add(state);
            }
            return states;
        }
    }
}
