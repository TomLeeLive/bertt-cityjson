﻿using CityJSON.Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using System.Linq;

namespace CityJSON.Tests;
public class CityJsonSeqTests
{
    [TestCase("fixtures/cityjsonseq/3dbag_b2.city.jsonl", "CityJSON", "2.0", "MultiPolygon")]
    [TestCase("fixtures/cityjsonseq/sisteron.city.jsonl", "CityJSON", "2.0", "MultiPolygon")]
    [TestCase("fixtures/cityjsonseq/montréal_b4.city.jsonl", "CityJSON", "2.0", "MultiPolygon")]
    public void ReadCityJsonSqFile(string filePath, string expectedType, string expectedVersion, string expectedGeometryType)
    {
        var cityJsondocuments = CityJsonSeqReader.ReadCityJsonSeq(filePath);
        var firstCityJsonDocument = cityJsondocuments.First();
        Assert.That(firstCityJsonDocument.Type == expectedType);
        Assert.That(firstCityJsonDocument.Version == expectedVersion);
        var transform = firstCityJsonDocument.Transform;

        // skip the first document
        foreach (var cityJsonSecond in cityJsondocuments.Skip(1))
        {
            var features = cityJsonSecond.ToFeatures(transform);
            Assert.That(features.Count > 0);
            var feature = features.First(); 
            var geom = feature.Geometry;
            Assert.That(geom.GeometryType == expectedGeometryType);
        }
    }

    [Test]
    public void ReadParisTower()
    {
        var cityJsondocuments = CityJsonSeqReader.ReadCityJsonSeq("./fixtures/cityjsonseq/paris_tower.city.jsonl");
        var firstCityJsonDocument = cityJsondocuments.First();

        var cityJsonSecond = cityJsondocuments.Skip(1).First();
        Assert.That(cityJsonSecond.Vertices.Count == 808);
        Assert.That(cityJsonSecond.CityObjects.Count == 2);
        Assert.That(cityJsonSecond.CityObjects.First().Value.Type == CityObjectType.Building);

        var features = cityJsonSecond.ToFeatures(firstCityJsonDocument.Transform);
        Assert.That(features.Count == 2);
        var feature = features.First();
        var geom = feature.Geometry;
        Assert.That(geom.GeometryType == "MultiPolygon");
        Assert.That(geom.NumGeometries == 1);

        var wktWriter = new WKTWriter();
        wktWriter.OutputOrdinates = Ordinates.XYZ;
        var wkt = wktWriter.Write(feature.Geometry);
    }
}
