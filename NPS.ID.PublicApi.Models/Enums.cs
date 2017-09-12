using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace NPS.ID.PublicApi.Models
{
    [Description(@"ACTI: Active
                   IACT: Expired(will never be reopened)
                   HIBE: Closed(can be reopened)")]
    public enum ContractStateEnum
    {
        [Description("Closed (can be reopened)")]
        HIBE,
        [Description("Expired (will never be reopened)")]
        IACT,
        [Description("Active")]
        ACTI
    }
    public enum ErrorTypeEnum
    {
        NPM,
        PRE_TRADE,
        MW,
        LTS,
        XBID
    }
    public enum ErrorCodeEnum
    {
        MISSING_REQUIRED_FIELD,
        ILLEGAL_FIELD,
        FIELD_OUT_OF_RANGE,
        FIELD_FORMAT_INVALID,
        ITEM_NOT_FOUND,
        ACCESS_MODE_VIOLATION,
        THIRD_PARTY
    }
    public enum OrderSideEnum
    {
        BUY,
        SELL
    }
    public enum TradeStateEnum
    {
        COMPLETED,
        DISPUTED,
        NOT_CANCELLED,
        CANCELLED
    }
    public enum LegOwnershipEnum
    {
        OWN_BUY,
        OWN_SELL,
        COMPANY_BUY,
        COMPANY_SELL,
        OTHER
    }
    public enum OrderTypeEnum
    {
        LIMIT,
        ICEBERG,
        USER_DEFINED_BLOCK

    }
    public enum TimeInForceEnum
    {
        IOC,
        FOK,
        NON,
        GTD,
        GFS
    }
    public enum OrderStateEnum
    {
        PENDING,
        ACTI,
        HIBE,
        IACT,
        REJECTED
    }
    public enum OrderActionEnum
    {
        USER_ADDED,
        USER_HIBERNATED,
        USER_MODIFIED,
        USER_DELETED,
        SYSTEM_HIBERNATED,
        SYSTEM_MODIFIED,
        SYSTEM_DELETED,
        PARTIAL_EXECUTION,
        FULL_EXECUTION,
        ICEBERG_SLICE_ADDED
    }
    public enum ExecutionRestrictionEnum
    {
        AON,
        NON
    }
    public enum ProductTypeEnum
    {
        P15MIN,
        P30MIN,
        P60MIN,
        BLOCK_2H,
        BLOCK_4H,
        DON,
        DB34,
        DP,
        DEP,
        DB,
        CUSTOM_BLOCK

        //P15MIN("QH", "Quarterly_Hour_Power"),
        //P30MIN("HH", "Half_Hourly_Power"),
        //P60MIN("PH", "Intraday_Power_D"),
        //BLOCK_2H("2H", ""),
        //BLOCK_4H("4H", ""),
        //DON("DON", ""),
        //DB34("DB34", ""),
        //DP("DP", "Continuous_Power_Peak"),
        //DEP("DEP", ""),
        //DB("DB", "Continuous_Power_Base"),
        //CUSTOM_BLOCK("CB", "");
    }
    public enum TendencyEnum
    {
        UP,
        DOWN,
        EQUAL
    }
}
