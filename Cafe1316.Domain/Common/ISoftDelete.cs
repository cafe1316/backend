namespace Cafe1316.Domain.Common;

public interface ISoftDelete
{
    DateTime? DeletedAt{get; set;}
}