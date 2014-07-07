using System.Collections.Generic;
using QSharp.Shared;

namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    using Classical.Sequential;

    public class RootSection : Section, ISection
    {
        #region Fields

        /// <summary>
        ///  The sream that underlies the section
        /// </summary>
        public readonly IStream SectionStream;

        /// <summary>
        ///  starting point of a section; non-paginated
        /// </summary>
        public readonly IPosition SectionStart;

        /// <summary>
        ///  section size; non-paginated
        /// </summary>
        public readonly ISize SectionSize;

        public readonly IRootEncoder Encoder;

        public bool Ready = false;
       
        public bool Dirty = false;

        public List<IPosition> Roots = new List<IPosition>();
        public List<Hole> Holes = new List<Hole>();

        #endregion

        #region Constructors

        /// <summary>
        ///  Instantiates a root section with the specfied traits
        /// </summary>
        /// <param name="sectionStream">The underlying stream</param>
        /// <param name="sectionStart">The starting point of the section</param>
        /// <param name="sectionSize">The size of the section</param>
        /// <param name="encoder">The encoder for the section</param>
        /// <remarks>
        ///  Both sectionStart and SectionSize are non-paginated variables.
        /// </remarks>
        public RootSection(IStream sectionStream, IPosition sectionStart, 
            ISize sectionSize, IRootEncoder encoder)
        {
            SectionStream = sectionStream;
            SectionStart = sectionStart;
            SectionSize = sectionSize;

            Encoder = encoder;
        }

        ~RootSection()
        {
            Close();
        }

        #endregion

        #region Methods

        #region ISection members

        public void Encode()
        {
            SectionStream.Position = SectionStart;
            Encoder.Encode(Holes, Roots, SectionStream);
            Dirty = false;
            Ready = true;
        }

        public void Decode()
        {
            SectionStream.Position = SectionStart;
            Encoder.Decode(Holes, Roots, SectionStream);
            Dirty = false;
            Ready = true;
        }

        public List<IPosition> AllocateForNodes(ISize size)
        {
            var posList = new List<IPosition>();

            var runLoop = true;
            while (runLoop)
            {
                Hole hole;
                if (Holes.Count == 0)
                {
                    throw new QException("Unexpected node page allocation failure");
                }

                int index;
                var found = Hole.Search(Holes, size, out index);

                if (found || index < Holes.Count)
                {

                    hole = Holes[index];
                    Holes.RemoveAt(index);
                    Hole newHole;
                    AllocatePages(hole, size, out newHole);

                    if (newHole != null)
                    {
                        Holes.Search(newHole, out index);
                        Holes.Insert(index, newHole);
                    }

                    runLoop = false;
                }
                else
                {
                    index = Holes.Count - 1;
                    hole = Holes[index];

                    Holes.RemoveAt(index);

                    size = TargetPaginator.Subtract(size, hole.Size);    // update remaining size
                }

                var nodePos = hole.Start;
                var nodePosEnd = TargetPaginator.Add(hole.Start, hole.Size);
                while (nodePos.CompareTo(nodePosEnd) < 0)
                {
                    posList.Add(nodePos);
                }
            }

            Dirty = true;

            return posList;
        }

        public IPosition Allocate(ISize size)
        {
            int index;
            var found = Hole.Search(Holes, size, out index);

            if (!found && index >= Holes.Count)
            {   // cannot provide the requested space
                return null;
            }

            var hole = Holes[index];

            Holes.RemoveAt(index);

            Hole newHole;
            Allocate(hole, size, out newHole);

            if (newHole != null)
            {
                Holes.Search(newHole, out index);
                Holes.Insert(index, newHole);
            }

            // the section stream needs to be updated
            Dirty = true;

            return hole.Start;
        }

        public void Deallocate(IPosition pos)
        {
            Hole oldHole1, oldHole2;
            Hole newHole;
            int index;

            Deallocate(pos, out oldHole1, out oldHole2, out newHole);

            if (oldHole1 != null)
            {
                Holes.Search(oldHole1, out index);
                if (index < 0)
                    throw new QException("Bad hole table");
                Holes.RemoveAt(index);
            }

            if (oldHole2 != null)
            {
                Holes.Search(oldHole2, out index);
                if (index < 0)
                    throw new QException("Bad hole table");
                Holes.RemoveAt(index);
            }

            if (Holes.Search(newHole, out index))
                throw new QException("Bad hole table");

            Holes.Insert(index, newHole);

            // the section stream needs to be updated
            Dirty = true;
        }

        #endregion

        /// <summary>
        ///  Part of resetting the entire management
        ///  Must be called after the root list is properly reset.
        /// </summary>
        public void Reset()
        {
            var start = TargetStart;
            var size = TargetSize;

            var inferiorSection = Inferior as BTreeSection;
            if (inferiorSection != null)
            {
                // the target stream contains one used chunk (root) initially
                start = TargetPaginator.Add(start, TargetPaginator.OnePage);
                size = TargetPaginator.Subtract(size, TargetPaginator.OnePage);
            }

            Holes.Clear();
            Holes.Add(new Hole(start, size));
            SectionStream.Position = SectionStart;

            Encoder.Encode(Holes, Roots, SectionStream);
        }

        public void Close()
        {
            if (Dirty)
            {
                Encode();   // writeback
            }
        }

        #endregion
    }
}
