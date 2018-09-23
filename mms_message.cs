
using System.Collections.Generic;
using NUnit.Framework;

namespace wp2droidMsg
{
    public class MmsPart
    {
        private int m_nSeq;
        private string m_sCt;
        private string m_sName;
        private string m_sChset;
        private string m_sCd;
        private string m_sFn;
        private string m_sCid;
        private string m_sCl;
        private string m_sCtt_s;
        private string m_sCtt_t;
        private string m_sText;
        private string m_sData; //optional

    }

    public class MmsAddress
    {
        // there is no schema for this published, this is just observed from a backup file
        private string m_sAddress;
        private int m_nType;
        private int m_nCharset;

        // from PduHeaders class:
        public static int s_AddressTypeFrom = 137;
        public static int s_AddressTypeBCC = 129;
        public static int s_AddressTypeCC = 130;
        public static int s_AddressTypeTo = 151;

        public MmsAddress(string sAddress, int nType, int nCharset)
        {
            m_sAddress = sAddress;
            m_nType = nType;
            m_nCharset = nCharset;
        }
    }

    public class MmsMessage
    {
        private List<MmsAddress> m_plAddresses;
        private List<MmsPart> m_plParts;

        // only from the observed file (a backup from an android phone using syntech's message backup
        private int m_nSpam_report;
        private string m_sMsg_id;
        private string m_sApp_id;
        private string m_sFrom_address;
        private string m_sSub_id; // subject id
        private int m_nReserved;
        private int m_nUsing_mode;
        private int m_nRr_st;
        private int m_nFavorite;
        private int m_nHidden;
        private int m_nDeletable;
        private int m_nD_rpt_st; // delivery report status
        private int m_nCallback_set;
        private string m_sDevice_name;
        private int m_nSim_slot;
        private string m_sCreator;
        private string m_sSim_imsi;
        private int m_nSafe_message;
        private int m_nSecret_mode;

        // from the schema http://synctech.com.au/wp-content/uploads/2018/01/sms.xsd_.txt AND observed file
        // comments about what the fields mean inferred from https://www.programcreek.com/java-api-examples/?code=ivanovpv/darksms/darksms-master/psm/src/main/java/ru/ivanovpv/gorets/psm/mms/pdu/PduPersister.java
        private ulong m_ulDate; // required, date
        private string m_sCt_t; // required, content type
        private string m_sCt_l; // required, content location
        private string m_sCt_cls; // required, content class
        private int m_nMsg_box; // required, message box
        private long m_lAddress; // required
        private string m_sSub_cs; // required, subject charset
        private string m_sRetr_st; // required, retrieve status
        private string m_sD_tm; // required, delivery time
        private string m_sExp; // required, expiry
        private int m_nLocked; // required
        private string m_sM_id; // required, message id
        private string m_sRetr_txt; // required, retrieve text
        private int m_nDate_sent; // required, date sent
        private int m_nRead; // required, read
        private string m_sRpt_a; // required, report allowed
        private int m_nPri; // required, priority
        private string m_sResp_txt; // required, response text
        private int m_nD_rpt; // required, delivery report
        private int m_nM_type; // required, message type
        private int m_nRr; // required, read report
        private string m_sSub; // optional, subject
        private string m_sRead_status; // required, read status
        private int m_nSeen;
        private string m_sResp_st; // required
        private int? m_nText_only; // optional
        private string m_sSt; // required, status
        private string m_sRetr_txt_cs; // required, retreive text charset
        private string m_sTr_id; // required, transation id
        private string m_sM_cls; // required, message class
        private int m_nV; // required, version
        private string m_sM_size; // required, message size
        private string m_sReadable_date; // optional
        private string m_sContact_name; // optional

        // charsets from https://www.iana.org/assignments/character-sets/character-sets.xhtml

        public static int s_CHARSET_USASCII = 3;
        public static int s_CHARSET_UTF8 = 106;

        // from https://kernel.googlesource.com/pub/scm/network/ofono/mmsd/+/b93b6037b7b0b3964c28a1c0721f6726e7c1cf21/src/mmsutil.h
        public static int s_MMS_MESSAGE_TYPE_SEND_REQ = 128;
        public static int s_MMS_MESSAGE_TYPE_SEND_CONF = 129;
        public static int s_MMS_MESSAGE_TYPE_NOTIFICATION_IND = 130;
        public static int s_MMS_MESSAGE_TYPE_NOTIFYRESP_IND = 131;
        public static int s_MMS_MESSAGE_TYPE_RETRIEVE_CONF = 132;
        public static int s_MMS_MESSAGE_TYPE_ACKNOWLEDGE_IND = 133;
        public static int s_MMS_MESSAGE_TYPE_DELIVERY_IND = 134;

        public static int s_MMS_MESSAGE_RSP_STATUS_OK = 128;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_UNSUPPORTED_MESSAGE = 136;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_TRANS_FAILURE = 192;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_TRANS_NETWORK_PROBLEM = 195;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_FAILURE = 224;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_SERVICE_DENIED = 225;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_MESSAGE_FORMAT_CORRUPT = 226;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_SENDING_ADDRESS_UNRESOLVED = 227;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_CONTENT_NOT_ACCEPTED = 229;
        public static int s_MMS_MESSAGE_RSP_STATUS_ERR_PERM_LACK_OF_PREPAID = 235;

        public static int s_MMS_MESSAGE_VERSION_1_0 = 0x90;
        public static int s_MMS_MESSAGE_VERSION_1_1 = 0x91;
        public static int s_MMS_MESSAGE_VERSION_1_2 = 0x92;
        public static int s_MMS_MESSAGE_VERSION_1_3 = 0x93;

        public static int s_MMS_EXPIRY_DEFAULT = 604800;

        public static int s_MESSAGEBOX_RECEIVED = 1;
        public static int s_MESSAGEBOX_SENT = 2;

        public MmsMessage() { }

        public static MmsMessage CreateFromWpMmsMessage(WpMessage wpm)
        {
            MmsMessage mms = new MmsMessage();

            // first, convert the addresses
            mms.m_plAddresses = new List<MmsAddress>();
            // first, add an address for the sender

            foreach (string s in wpm.Recipients)
                mms.m_plAddresses.Add(new MmsAddress(s, MmsAddress.s_AddressTypeTo, s_CHARSET_UTF8));

            if (wpm.Incoming) // type 1
            {
                mms.m_plAddresses.Add(new MmsAddress(wpm.Sender, MmsAddress.s_AddressTypeFrom, s_CHARSET_UTF8));
                mms.m_nM_type = s_MMS_MESSAGE_TYPE_SEND_REQ;
                mms.m_nMsg_box = s_MESSAGEBOX_RECEIVED;
                mms.m_sExp = "null";
            }
            else
            {
                // if its not incoming, then the sender is ourselves
                mms.m_plAddresses.Add(new MmsAddress("insert-address-token", MmsAddress.s_AddressTypeFrom, s_CHARSET_UTF8));
                mms.m_nM_type = s_MMS_MESSAGE_TYPE_RETRIEVE_CONF;
                mms.m_nMsg_box = s_MESSAGEBOX_SENT;
                mms.m_sExp = "604800"; // 7 days
            }

            mms.m_nRead = wpm.Read ? 1 : 0;

            return mms;
        }
    }
}