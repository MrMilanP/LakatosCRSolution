using PCSC;
using PCSC.Exceptions;
using PCSC.Monitoring;
using System;
using System.Collections.Generic;
using LakatosCardReader.Interfaces;
using LakatosCardReader.Models;
using LakatosCardReader.Utils;

namespace LakatosCardReader.CardReader
{
    public class LIdentityCardReader : ILIdentityCardReader
    {

        private readonly ILIdentityDataParser _parser;
        private readonly ILCardReader _cardReader;

        public LIdentityCardReader(ILIdentityDataParser parser, ILCardReader cardReader)
        {
            _parser = parser;
            _cardReader = cardReader;
        }


        public LIdentityCardReadResult ReadIdentityCardData(string readerName)
        {


            LIdentityCardReadResult result = new LIdentityCardReadResult();

            if (!_cardReader.IsStarted())
            {
                result.Success = false;
                result.ErrorMessage = "Card reader is not started.";
                return result;
            }

            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    // Selektujemo AID
                    var aids = new List<byte[]>
                    {
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x44, 0x01 }, //ID Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x46, 0x01 }, //IF Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x52, 0x50, 0x01 }  //RP Card
                    };

                    bool aidSelected = false;
                    foreach (var aid in aids)
                    {
                        var selectAid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, aid, 0);
                        var rsp = ApduHelper.Transmit(reader, selectAid);
                        if (ApduHelper.IsResponseOK(rsp))
                        {
                            aidSelected = true;
                            break;
                        }
                    }

                    if (!aidSelected)
                    {
                        result.Success = false;
                        result.ErrorMessage = "No AID selected.";
                        return result;
                    }

                    // Čitaj fajlove
                    byte[] documentFileData = ApduHelper.ReadFile(reader, new byte[] { 0x0F, 0x02 });
                    byte[] personalFileData = ApduHelper.ReadFile(reader, new byte[] { 0x0F, 0x03 });
                    byte[] residenceFileData = ApduHelper.ReadFile(reader, new byte[] { 0x0F, 0x04 });
                    byte[] photoData = ApduHelper.ReadFile(reader, new byte[] { 0x0F, 0x06 });

                    // Parsiraj
                    LIdentityCardModel cardModel = new LIdentityCardModel();

                    cardModel.Document = _parser.ParseDocument(documentFileData);

                    cardModel.FixedPersonal = _parser.ParsePersonal(personalFileData, cardModel);
                    cardModel.VariablePersonal = _parser.ParseResidence(residenceFileData, cardModel);

                    if (photoData?.Length > 0)
                    {
                        cardModel.Portrait = _parser.ParsePhoto(photoData);
                    }

                    result.IdentityCardData = cardModel;
                    result.Success = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        public async Task<LIdentityCardReadResult> ReadIdentityCardDataAsync(string readerName)
        {


            LIdentityCardReadResult result = new LIdentityCardReadResult();

            if (!_cardReader.IsStarted())
            {
                result.Success = false;
                result.ErrorMessage = "Card reader is not started.";
                return result;
            }

            try
            {
                using (var context = ContextFactory.Instance.Establish(SCardScope.System))
                using (var reader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any))
                {
                    // Selektujemo AID
                    var aids = new List<byte[]>
                    {
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x44, 0x01 }, //ID Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x49, 0x46, 0x01 }, //IF Card
                        new byte[] { 0xF3, 0x81, 0x00, 0x00, 0x02, 0x53, 0x45, 0x52, 0x52, 0x50, 0x01 }  //RP Card
                    };

                    bool aidSelected = false;
                    foreach (var aid in aids)
                    {
                        var selectAid = APDUBuilder.BuildAPDU(0x00, 0xA4, 0x04, 0x00, aid, 0);
                        var rsp = await ApduHelper.TransmitAsync(reader, selectAid);
                        if (ApduHelper.IsResponseOK(rsp))
                        {
                            aidSelected = true;
                            break;
                        }
                    }

                    if (!aidSelected)
                    {
                        result.Success = false;
                        result.ErrorMessage = "No AID selected.";
                        return result;
                    }

                    // Čitaj fajlove
                    byte[] documentFileData = await ApduHelper.ReadFileAsync(reader, new byte[] { 0x0F, 0x02 });
                    byte[] personalFileData = await ApduHelper.ReadFileAsync(reader, new byte[] { 0x0F, 0x03 });
                    byte[] residenceFileData = await ApduHelper.ReadFileAsync(reader, new byte[] { 0x0F, 0x04 });
                    byte[] photoData = await ApduHelper.ReadFileAsync(reader, new byte[] { 0x0F, 0x06 });

                    // Parsiraj
                    LIdentityCardModel cardModel = new LIdentityCardModel();

                    cardModel.Document = _parser.ParseDocument(documentFileData);

                    cardModel.FixedPersonal = _parser.ParsePersonal(personalFileData, cardModel);
                    cardModel.VariablePersonal = _parser.ParseResidence(residenceFileData, cardModel);

                    if (photoData?.Length > 0)
                    {
                        cardModel.Portrait = _parser.ParsePhoto(photoData);
                    }

                    result.IdentityCardData = cardModel;
                    result.Success = true;
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

    }
}
