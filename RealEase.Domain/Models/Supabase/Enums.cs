using System.Runtime.Serialization;

namespace RealEase.Domain.Models.Supabase
{
    public enum UserRole
    {
        [EnumMember(Value = "guest")]
        Guest,

        [EnumMember(Value = "host")]
        Host,

        [EnumMember(Value = "administrator")]
        Administrator
    }

    public enum ReservationStatus
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "confirmed")]
        Confirmed,

        [EnumMember(Value = "cancelled")]
        Cancelled,

        [EnumMember(Value = "completed")]
        Completed
    }
}
