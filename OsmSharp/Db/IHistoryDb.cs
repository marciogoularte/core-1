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

using OsmSharp.Changesets;
using OsmSharp.Streams;
using System.Collections.Generic;

namespace OsmSharp.Db
{
    /// <summary>
    /// Abstract representation of a database to store OSM-data including history. This db can store nodes, ways, relations and changesets.
    /// 
    /// - Multiple version of the same node can exist.
    /// - Changes are only possible by:
    ///    - Applying changesets.
    /// - Changeset meta-data can only be updated when changesets are open.
    /// - Changesets have to applied in the following order:
    ///    - 1. Open new changeset, an new id is returned.
    ///    - 2. Apply changes.
    ///    - 3. Close the changeset.
    /// </summary>
    public interface IHistoryDb : IOsmGeoSource
    {
        /// <summary>
        /// Clears all data.
        /// </summary>
        void Clear();

        /// <summary>
        /// Adds the given osm object in the db exactly as given.
        /// </summary>
        /// <remarks>
        /// - To update use ApplyChanges.
        /// - It's not possible to add multiple objects of a given type with the same id/version# pair.
        /// </remarks>
        void Add(OsmGeo osmGeo);

        /// <summary>
        /// Adds osm objects in the db exactly as they are given.
        /// </summary>
        /// <remarks>
        /// - To update use ApplyChanges.
        /// - It's not possible to add multiple objects of a given type with the same id/version# pair.
        /// </remarks>
        void Add(IEnumerable<OsmGeo> osmGeos);

        /// <summary>
        /// Adds the given changeset in the db exactly as given.
        /// </summary>
        void Add(Changeset meta, OsmChange changes);

        /// <summary>
        /// Gets all the objects in the form of an osm stream source.
        /// </summary>
        OsmStreamSource Get();

        /// <summary>
        /// Gets all latest versions of osm objects with the given types and the given id's.
        /// </summary>
        IList<OsmGeo> Get(IList<OsmGeoType> type, IList<long> id);

        /// <summary>
        /// Gets an osm object of the given type, the given id and the given version #.
        /// </summary>
        OsmGeo Get(OsmGeoType type, long id, int version);

        /// <summary>
        /// Gets all osm objects with the given types, the given id's and the given version #'s.
        /// </summary>
        IList<OsmGeo> Get(IList<OsmGeoType> type, IList<long> id, IList<int> version);

        /// <summary>
        /// Gets all latest versions of osm objects within the given bounding box.
        /// </summary>
        IList<OsmGeo> Get(float minLatitude, float minLongitude, float maxLatitude, float maxLongitude);

        /// <summary>
        /// Opens a new changeset
        /// </summary>
        long OpenChangeset(Changeset info);

        /// <summary>
        /// Applies the given changeset.
        /// </summary>
        /// <param name="id">The id of the changeset to apply the changes for.</param>
        /// <param name="changeset">The changes to apply.</param>
        /// <param name="bestEffort">When false, it's the entire changeset or nothing. When true the changeset is applied using best-effort.</param>
        /// <returns>The diff result result object containing the diff result and status information.</returns>
        DiffResultResult ApplyChangeset(long id, OsmChange changeset, bool bestEffort = false);

        /// <summary>
        /// Updates the changeset with the new info.
        /// </summary>
        bool UpdateChangesetInfo(Changeset info);

        /// <summary>
        /// Closes the changeset with the given id.
        /// </summary>
        bool CloseChangeset(long id);
    }
}