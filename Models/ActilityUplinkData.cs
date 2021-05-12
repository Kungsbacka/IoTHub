using System;

namespace IoTHub.Model
{
    public class ActilityUplinkData
    {
        public DevEUI_Uplink DevEUI_Uplink { get; set; }
    }

    public class DevEUI_Uplink
    {
        public DateTime Time { get; set; }
        public string DevEUI { get; set; }
        public int FPort { get; set; }
        public int FCntUp { get; set; }
        public int ADRbit { get; set; }
        public int MType { get; set; }
        public int FCntDn { get; set; }
        public string Payload_Hex { get; set; }
        public string Mic_Hex { get; set; }
        public string Lrcid { get; set; }
        public float LrrRSSI { get; set; }
        public float LrrSNR { get; set; }
        public int SpFact { get; set; }
        public string SubBand { get; set; }
        public string Channel { get; set; }
        public int DevLrrCnt { get; set; }
        public string Lrrid { get; set; }
        public int Late { get; set; }
        public float LrrLAT { get; set; }
        public float LrrLON { get; set; }
        public Lrrs Lrrs { get; set; }
        public string CustomerID { get; set; }
        public Customerdata CustomerData { get; set; }
        public string ModelCfg { get; set; }
        public float InstantPER { get; set; }
        public float MeanPER { get; set; }
        public string DevAddr { get; set; }
        public float TxPower { get; set; }
        public int NbTrans { get; set; }
        public float Frequency { get; set; }
        public string DynamicClass { get; set; }
    }

    public class Lrrs
    {
        public Lrr[] Lrr { get; set; }
    }

    public class Lrr
    {
        public string Lrrid { get; set; }
        public int Chain { get; set; }
        public float LrrRSSI { get; set; }
        public float LrrSNR { get; set; }
        public float LrrESP { get; set; }
    }

    public class Customerdata
    {
        public Alr Alr { get; set; }
    }

    public class Alr
    {
        public string Pro { get; set; }
        public string Ver { get; set; }
    }
}
