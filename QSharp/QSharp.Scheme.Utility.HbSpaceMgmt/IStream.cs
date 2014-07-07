namespace QSharp.Scheme.Utility.HbSpaceMgmt
{
    public interface IStream
    {
        #region Properties

        IPosition Position { get; set; }

        #endregion

        #region Methods

        void Write(byte[] buffer, int offset, int count);

        int Read(byte[] buffer, int offset, int count);

        #endregion
    }
}
