using LakatosCardReader.Models;
using SixLabors.ImageSharp;

namespace LakatosCardReader.Interfaces
{
    public interface ILIdentityDataParser
    {
        LIdentityCardModel.DocumentData ParseDocument(byte[] documentFileData);
        LIdentityCardModel.FixedPersonalData ParsePersonal(byte[] personalFileData, LIdentityCardModel currentModel);
        LIdentityCardModel.VariablePersonalData ParseResidence(byte[] residenceFileData, LIdentityCardModel currentModel);
        Image? ParsePhoto(byte[] photoData);
    }
}