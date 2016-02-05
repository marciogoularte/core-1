﻿// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Geometries;
using OsmSharp.Osm;
using OsmSharp.Osm.Data;
using OsmSharp.Osm.Geo.Interpreter;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Test.Osm.Geo.Interpreter
{
    /// <summary>
    /// Contains tests for the default feature interpreter class testing as many of the openstreetmap tags ->  geometry logic as possible.
    /// </summary>
    [TestFixture]
    public class SimpleFeatureInterpreterTests
    {
        /// <summary>
        /// Tests the interpretation of an area.
        /// Way(area=yes) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Area#A_simple_area
        /// </summary>
        [Test]
        public void TestWayAreaIsYesArea()
        {
            var node1 = new Node();
            node1.Id = 1;
            node1.Latitude = 0;
            node1.Longitude = 0;
            var node2 = new Node();
            node2.Id = 2;
            node2.Latitude = 1;
            node2.Longitude = 0;
            var node3 = new Node();
            node3.Id = 3;
            node3.Latitude = 0;
            node3.Longitude = 1;

            var way = new Way();
            way.Id = 1;
            way.Nodes = new List<long>();
            way.Nodes.Add(1);
            way.Nodes.Add(2);
            way.Nodes.Add(3);
            way.Nodes.Add(1);
            way.Tags = new TagsCollection();
            way.Tags.Add("area", "yes");

            var source = new MemorySnapshotDb();
            source.AddOrUpdate(node1);
            source.AddOrUpdate(node2);
            source.AddOrUpdate(node3);
            source.AddOrUpdate(way);

            // the use of natural=water implies an area-type.
            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(way, source);

            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<LineairRing>(feature.Geometry);
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("area", "yes"));
        }

        /// <summary>
        /// Tests the interpretation of a water area.
        /// Way(natural=water) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Tag:natural=water
        /// </summary>
        [Test]
        public void TestWayNaturalIsWaterArea()
        {
            var node1 = new Node();
            node1.Id = 1;
            node1.Latitude = 0;
            node1.Longitude = 0;
            var node2 = new Node();
            node2.Id = 2;
            node2.Latitude = 1;
            node2.Longitude = 0;
            var node3 = new Node();
            node3.Id = 3;
            node3.Latitude = 0;
            node3.Longitude = 1;

            var way = new Way();
            way.Id = 1;
            way.Nodes = new List<long>();
            way.Nodes.Add(1);
            way.Nodes.Add(2);
            way.Nodes.Add(3);
            way.Nodes.Add(1);
            way.Tags = new TagsCollection();
            way.Tags.Add("natural", "water");

            var source = new MemorySnapshotDb();
            source.AddOrUpdate(node1);
            source.AddOrUpdate(node2);
            source.AddOrUpdate(node3);
            source.AddOrUpdate(way);

            // the use of natural=water implies an area-type.
            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(way, source);

            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<LineairRing>(feature.Geometry);
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("natural", "water"));
        }

        /// <summary>
        /// Tests the interpretation of a multipolygon relation.
        /// Relation(type=multipolygon) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Relation:multipolygon
        /// </summary>
        [Test]
        public void TestRelationMultipolygonAreaOneOuter()
        {
            // tests a multipolygon containing one 'outer' member.
            var source = new MemorySnapshotDb(
                new Node(1, 0, 0),
                new Node(2, 1, 0),
                new Node(3, 0, 1),
                Way.Create(1, 1, 2, 3, 1),
                Relation.Create(1, 
                    new TagsCollection(
                        Tag.Create("type", "multipolygon")),
                    RelationMember.Create(1, "outer", OsmGeoType.Way)));

            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(source.GetRelation(1), source);
            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<LineairRing>(feature.Geometry);
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("type", "multipolygon"));
        }

        /// <summary>
        /// Tests the interpretation of a multipolygon containing one 'outer' member and one 'inner' member.
        /// Relation(type=multipolygon) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Relation:multipolygon
        /// </summary>
        [Test]
        public void TestRelationMultipolygonAreaOneOuterOneInner()
        {
            var source = new MemorySnapshotDb(
                new Node(1, 0, 0),
                new Node(2, 0, 1),
                new Node(3, 1, 1),
                new Node(4, 1, 0),
                new Node(5, 0.25f, 0.25f),
                new Node(6, 0.25f, 0.40f),
                new Node(7, 0.40f, 0.40f),
                new Node(8, 0.40f, 0.25f),
                Way.Create(1, 1, 2, 3, 4, 1),
                Way.Create(2, 5, 6, 7, 8, 5),
                Relation.Create(1,
                    new TagsCollection(
                        Tag.Create("type", "multipolygon")),
                    RelationMember.Create(1, "outer", OsmGeoType.Way),
                    RelationMember.Create(2, "inner", OsmGeoType.Way)));

            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(source.GetRelation(1), source);
            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<Polygon>(feature.Geometry);
            var polygon = feature.Geometry as Polygon;
            Assert.IsNotNull(polygon.Holes);
            Assert.AreEqual(1, polygon.Holes.Count());
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("type", "multipolygon"));
        }
        
        /// <summary>
        /// Tests the interpretation of a multipolygon containing one 'outer' member and two 'inner' members.
        /// Relation(type=multipolygon) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Relation:multipolygon
        /// </summary>
        [Test]
        public void TestRelationMultipolygonAreaOneOuterTwoInners()
        {
            var source = new MemorySnapshotDb(
                new Node(1, 0, 0),
                new Node(2, 0, 1),
                new Node(3, 1, 1),
                new Node(4, 1, 0),
                new Node(5, 0.25f, 0.25f),
                new Node(6, 0.25f, 0.40f),
                new Node(7, 0.40f, 0.40f),
                new Node(8, 0.40f, 0.25f),
                new Node(9, 0.60f, 0.25f),
                new Node(10, 0.60f, 0.40f),
                new Node(11, 0.75f, 0.40f),
                new Node(12, 0.75f, 0.25f),
                Way.Create(1, 1, 2, 3, 4, 1),
                Way.Create(2, 5, 6, 7, 8, 5),
                Way.Create(3, 9, 10, 11, 12, 9),
                Relation.Create(1,
                    new TagsCollection(
                        Tag.Create("type", "multipolygon")),
                    RelationMember.Create(1, "outer", OsmGeoType.Way),
                    RelationMember.Create(2, "inner", OsmGeoType.Way),
                    RelationMember.Create(3, "inner", OsmGeoType.Way)));

            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(source.GetRelation(1), source);
            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<Polygon>(feature.Geometry);
            Polygon polygon = feature.Geometry as Polygon;
            Assert.IsNotNull(polygon.Holes);
            Assert.AreEqual(2, polygon.Holes.Count());
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("type", "multipolygon"));
        }

        /// <summary>
        /// Tests the interpretation of a multipolygon containing one 'outer' member and two partial 'inner' member.
        /// Relation(type=multipolygon) => area
        /// 
        /// http://wiki.openstreetmap.org/wiki/Relation:multipolygon
        /// </summary>
        [Test]
        public void TestRelationMultipolygonAreaOneOuterTwoPartialInners()
        {
            var source = new MemorySnapshotDb(
                new Node(1, 0, 0),
                new Node(2, 0, 1),
                new Node(3, 1, 1),
                new Node(4, 1, 0),
                new Node(5, 0.25f, 0.25f),
                new Node(6, 0.25f, 0.40f),
                new Node(7, 0.40f, 0.40f),
                new Node(8, 0.40f, 0.25f),
                Way.Create(1, 1, 2, 3, 4, 1),
                Way.Create(2, 5, 6, 7),
                Way.Create(3, 7, 8, 5),
                Relation.Create(1,
                    new TagsCollection(
                        Tag.Create("type", "multipolygon")),
                    RelationMember.Create(1, "outer", OsmGeoType.Way),
                    RelationMember.Create(2, "inner", OsmGeoType.Way),
                    RelationMember.Create(3, "inner", OsmGeoType.Way)));

            var interpreter = new SimpleFeatureInterpreter();
            var features = interpreter.Interpret(source.GetRelation(1), source);
            Assert.IsNotNull(features);
            Assert.AreEqual(1, features.Count);
            var feature = features[0];
            Assert.IsInstanceOf<Polygon>(feature.Geometry);
            Polygon polygon = feature.Geometry as Polygon;
            Assert.IsNotNull(polygon.Holes);
            Assert.AreEqual(1, polygon.Holes.Count());
            Assert.IsTrue(feature.Attributes.ContainsKeyValue("type", "multipolygon"));
        }
    }
}