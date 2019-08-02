﻿// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using NUnit.Framework;
using OsmSharp.API;
using OsmSharp.IO.Xml;
using System;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace OsmSharp.Test.IO.Xml.API
{
    /// <summary>
    /// Contains tests for the osm class.
    /// </summary>
    [TestFixture]
    public class OsmTests
    {
        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var osm = new Osm()
            {
                Api = new Capabilities()
                {
                    Version = new OsmSharp.API.Version()
                    {
                        Maximum = 0.6,
                        Minimum = 0.6
                    },
                    Area = new Area()
                    {
                        Maximum = 0.25
                    },
                    Changesets = new OsmSharp.API.Changesets()
                    {
                        MaximumElements = 50000
                    },
                    Status = new Status()
                    {
                        Api = "online",
                        Database = "online",
                        Gpx = "online"
                    },
                    Timeout = new Timeout()
                    {
                        Seconds = 300
                    },
                    Tracepoints = new Tracepoints()
                    {
                        PerPage = 5000
                    },
                    WayNodes = new WayNodes()
                    {
                        Maximum = 2000
                    }
                }
            };

            Assert.AreEqual("<osm><api><version minimum=\"0.6\" maximum=\"0.6\" /><area maximum=\"0.25\" /><tracepoints per_page=\"5000\" /><waynodes maximum=\"2000\" /><changesets maximum_elements=\"50000\" /><timeout seconds=\"300\" /><status api=\"online\" database=\"online\" gpx=\"online\" /></api></osm>", 
                osm.SerializeToXml());
        }

        /// <summary>
        /// Test deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var serializer = new XmlSerializer(typeof(Osm));

            var osm = serializer.Deserialize(
                new StringReader("<osm><api><version minimum=\"0.6\" maximum=\"0.6\" /><area maximum=\"0.25\" /><tracepoints per_page=\"5000\" /><waynodes maximum=\"2000\" /><changesets maximum_elements=\"50000\" /><timeout seconds=\"300\" /><status api=\"online\" database=\"online\" gpx=\"online\" /></api></osm>")) 
                    as Osm;
            Assert.IsNotNull(osm);
            var capabilities = osm.Api;
            Assert.IsNotNull(capabilities);
            Assert.IsNotNull(capabilities.Version);
            Assert.AreEqual(0.6, capabilities.Version.Minimum);
            Assert.AreEqual(0.6, capabilities.Version.Maximum);
            Assert.IsNotNull(capabilities.Area);
            Assert.AreEqual(0.25, capabilities.Area.Maximum);
            Assert.IsNotNull(capabilities.Changesets);
            Assert.AreEqual(50000, capabilities.Changesets.MaximumElements);
            Assert.IsNotNull(capabilities.Status);
            Assert.AreEqual("online", capabilities.Status.Api);
            Assert.AreEqual("online", capabilities.Status.Database);
            Assert.AreEqual("online", capabilities.Status.Gpx);
            Assert.IsNotNull(capabilities.Timeout);
            Assert.AreEqual(300, capabilities.Timeout.Seconds);
            Assert.IsNotNull(capabilities.Tracepoints);
            Assert.AreEqual(5000, capabilities.Tracepoints.PerPage);
            Assert.IsNotNull(capabilities.WayNodes);
            Assert.AreEqual(2000, capabilities.WayNodes.Maximum);
        }

        /// <summary>
        /// Test deserialization of XML that contains unexpected elements (for example 'note' and 'meta' from an OverpassApi result).
        /// </summary>
        [Test]
        public void TestDeserializeSkippingUnexpectedElements()
        {
            var xml =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <osm version=""0.6"" generator=""Overpass API 0.7.55.7 8b86ff77"">
                    <note>This is just a note</note>
                    <meta osm_base=""2019-07-27T00:04:02Z"" areas=""2019-07-26T23:48:03Z""/>
                    <node id=""1"" lat=""111"" lon=""-70.111"">
                        <tag k=""addr:housenumber"" v=""11""/>
                        <tag k=""addr:street"" v=""Main Street""/>
                    </node>
                </osm>";
            
            var serializer = new XmlSerializer(typeof(Osm));
            var osm = serializer.Deserialize(new StringReader(xml)) as Osm;
            Assert.IsNotNull(osm);
            Assert.AreEqual(.6, osm.Version);
            Assert.AreEqual("Overpass API 0.7.55.7 8b86ff77", osm.Generator);

            Assert.IsNull(osm.Ways);
            Assert.IsNull(osm.Relations);
            Assert.IsNull(osm.User);
            Assert.IsNull(osm.GpxFiles);
            Assert.IsNull(osm.Bounds);
            Assert.IsNull(osm.Api);

            Assert.IsNotNull(osm.Nodes);
            Assert.AreEqual(1, osm.Nodes.Length);
            var node = osm.Nodes[0];
            Assert.AreEqual(1, node.Id);
            Assert.AreEqual(111, node.Latitude);
            Assert.AreEqual(-70.111, node.Longitude);
            Assert.NotNull(node.Tags);
            Assert.AreEqual(2, node.Tags.Count);
            Assert.True(node.Tags.ContainsKey("addr:housenumber"));
            Assert.AreEqual("11", node.Tags["addr:housenumber"]);
            Assert.True(node.Tags.ContainsKey("addr:street"));
            Assert.AreEqual("Main Street", node.Tags["addr:street"]);
        }

        /// <summary>
        /// Test deserialization of XML that contains bounds.
        /// </summary>
        [Test]
        public void TestDeserializeWithBoundsElement()
        {
            var xml =
                @"<?xml version=""1.0"" encoding=""UTF-8""?>
                <osm version=""0.6"" generator=""CGImap 0.7.5 (5035 errol.openstreetmap.org)"" copyright=""OpenStreetMap and contributors"" attribution=""http://www.openstreetmap.org/copyright"" license=""http://opendatacommons.org/licenses/odbl/1-0/"">
                 <bounds minlat=""38.9070200"" minlon=""-77.0371900"" maxlat=""38.9077300"" maxlon=""-77.0360000""/>
                 <node id=""8549479"" visible=""true"" version=""6"" changeset=""17339"" timestamp=""2013-01-20T06:31:24Z"" user=""samanbb"" uid=""933"" lat=""38.8921989"" lon=""-77.0503034""/>
                 <node id=""8549530"" visible=""false"" version=""2"" changeset=""17248"" timestamp=""2013-01-17T15:24:35Z"" user=""ideditor"" uid=""912"" lat=""38.9065506"" lon=""-77.0345080""/>
                 <way id=""538868"" visible=""true"" version=""5"" changeset=""23710"" timestamp=""2013-05-28T17:45:26Z"" user=""Kate"" uid=""1163"">
                  <nd ref=""4294969195""/>
                  <nd ref=""4294969575""/>
                  <tag k=""highway"" v=""residential""/>
                  <tag k=""maxspeed:practical"" v=""12.910093541777924""/>
                 </way>
                </osm>
                ";

            Func<string, DateTime> parseToUniversalTime =
                t => DateTime.Parse(t, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var serializer = new XmlSerializer(typeof(Osm));
            var osm = serializer.Deserialize(new StringReader(xml)) as Osm;
            Assert.IsNotNull(osm);
            Assert.AreEqual(.6, osm.Version);
            Assert.AreEqual("CGImap 0.7.5 (5035 errol.openstreetmap.org)", osm.Generator);

            Assert.IsNull(osm.Relations);
            Assert.IsNull(osm.User);
            Assert.IsNull(osm.GpxFiles);
            Assert.IsNull(osm.Api);

            Assert.IsNotNull(osm.Bounds);
            Assert.AreEqual(float.Parse("38.9070200"), osm.Bounds.MinLatitude);
            Assert.AreEqual(float.Parse("-77.0371900"), osm.Bounds.MinLongitude);
            Assert.AreEqual(float.Parse("38.9077300"), osm.Bounds.MaxLatitude);
            Assert.AreEqual(float.Parse("-77.0360000"), osm.Bounds.MaxLongitude);

            Assert.IsNotNull(osm.Nodes);
            Assert.AreEqual(2, osm.Nodes.Length);
            var node = osm.Nodes[0];
            Assert.AreEqual(8549479, node.Id);
            Assert.AreEqual(true, node.Visible);
            Assert.AreEqual(6, node.Version);
            Assert.AreEqual(17339, node.ChangeSetId);
            Assert.AreEqual(parseToUniversalTime("2013-01-20T06:31:24Z"), node.TimeStamp);
            Assert.AreEqual("samanbb", node.UserName);
            Assert.AreEqual(933, node.UserId);
            Assert.AreEqual(38.8921989, node.Latitude);
            Assert.AreEqual(-77.0503034, node.Longitude);
            Assert.IsNull(node.Tags);
            node = osm.Nodes[1];
            Assert.AreEqual(8549530, node.Id);
            Assert.AreEqual(false, node.Visible);
            Assert.AreEqual(2, node.Version);
            Assert.AreEqual(17248, node.ChangeSetId);
            Assert.AreEqual(parseToUniversalTime("2013-01-17T15:24:35Z"), node.TimeStamp);
            Assert.AreEqual("ideditor", node.UserName);
            Assert.AreEqual(912, node.UserId);
            Assert.AreEqual(38.9065506, node.Latitude);
            Assert.AreEqual(-77.0345080, node.Longitude);
            Assert.IsNull(node.Tags);

            Assert.IsNotNull(osm.Ways);
            Assert.AreEqual(1, osm.Ways.Length);
            var way = osm.Ways[0];
            Assert.AreEqual(538868, way.Id);
            Assert.AreEqual(true, way.Visible);
            Assert.AreEqual(5, way.Version);
            Assert.AreEqual(23710, way.ChangeSetId);
            Assert.AreEqual(parseToUniversalTime("2013-05-28T17:45:26Z"), way.TimeStamp);
            Assert.AreEqual("Kate", way.UserName);
            Assert.AreEqual(1163, way.UserId);
            Assert.NotNull(way.Nodes);
            Assert.AreEqual(2, way.Nodes.Length);
            Assert.AreEqual(4294969195, way.Nodes[0]);
            Assert.AreEqual(4294969575, way.Nodes[1]);
            Assert.NotNull(way.Tags);
            Assert.AreEqual(2, way.Tags.Count);
            Assert.True(way.Tags.ContainsKey("highway"));
            Assert.AreEqual("residential", way.Tags["highway"]);
            Assert.True(way.Tags.ContainsKey("maxspeed:practical"));
            Assert.AreEqual("12.910093541777924", way.Tags["maxspeed:practical"]);
        }
    }
}