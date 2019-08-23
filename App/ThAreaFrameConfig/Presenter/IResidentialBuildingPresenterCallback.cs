namespace ThAreaFrameConfig.Presenter
{
    public interface IResidentialBuildingPresenterCallback : IThAreaFramePresenterCallback
    {
        void OnRenameAreaFrameLayer(string name, string newName);
    }
}
