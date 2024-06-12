namespace cv_iidx_api
{
    public interface ISingletonCVContainer
    {
        Task<IIDXcvResults> ParseImage();
    }
}
