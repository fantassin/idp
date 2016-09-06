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

using IDP.Processors;
using IDP.Processors.RouterDb;
using IDP.Processors.TransitDb;
using System;
using System.Collections.Generic;

namespace IDP.Switches.MultimodalDb
{
    /// <summary>
    /// A switch to create a multimodal db.
    /// </summary>
    class SwitchCreateMultimodalDb : Switch
    {
        /// <summary>
        /// Creates a switch to create a multimodal db.
        /// </summary>
        public SwitchCreateMultimodalDb(string[] a)
            : base(a)
        {

        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        public static string[] Names
        {
            get
            {
                return new string[] { "--create-multimodaldb" };
            }
        }
        /// <summary>
        /// Parses this command into a processor given the arguments for this switch. Consumes the previous processors and returns how many it consumes.
        /// </summary>
        public override int Parse(List<Processor> previous, out Processor processor)
        {
            if (previous == null || previous.Count == 0) { throw new ArgumentOutOfRangeException("processors"); }

            // ok combine the transit db and the router db into one multimodal db.
            Func<Itinero.Transit.Data.TransitDb> getTransitDb;
            Func<Itinero.RouterDb> getRouterDb;
            if (previous[previous.Count - 2] is IProcessorTransitDbSource &&
                previous[previous.Count - 1] is IProcessorRouterDbSource)
            {
                getTransitDb = (previous[previous.Count - 2] as IProcessorTransitDbSource).GetTransitDb;
                getRouterDb = (previous[previous.Count - 1] as IProcessorRouterDbSource).GetRouterDb;
            }
            else if (previous[previous.Count - 1] is IProcessorTransitDbSource &&
                previous[previous.Count - 2] is IProcessorRouterDbSource)
            {
                getTransitDb = (previous[previous.Count - 1] as IProcessorTransitDbSource).GetTransitDb;
                getRouterDb = (previous[previous.Count - 2] as IProcessorRouterDbSource).GetRouterDb;
            }
            else
            {
                throw new Exception("Creating a multimodal requires a transit db an a router db source.");
            }
            previous.RemoveAt(previous.Count - 1);
            previous.RemoveAt(previous.Count - 2);

            processor = new Processors.MultimodalDb.ProcessorMultimodalDbSource(() =>
            {
                var transitDb = getTransitDb();
                var routerDb = getRouterDb();

                return new Itinero.Transit.Data.MultimodalDb(routerDb, transitDb);
            });

            return 2;
        }
    }
}