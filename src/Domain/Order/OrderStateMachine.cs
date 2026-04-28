using Domain.Users;

namespace Domain.Order;

public static class OrderStateMachine
{
    private static readonly Dictionary<string, HashSet<(int From, int To)>> _transitions = new()
    {
        [UserRole.BusinessOwner.Name] = new HashSet<(int, int)>
                    {
                        (EOrderStatus.Pending.Id, EOrderStatus.Accepted.Id),
                        (EOrderStatus.Accepted.Id, EOrderStatus.Preparing.Id),
                        (EOrderStatus.Preparing.Id, EOrderStatus.ReadyForPickup.Id),
                         (EOrderStatus.ReadyForPickup.Id, EOrderStatus.Delivered.Id)
                    },
        [UserRole.Rider.Name] = new HashSet<(int, int)>
                    {
                        (EOrderStatus.ReadyForPickup.Id, EOrderStatus.InTransit.Id),
                        (EOrderStatus.InTransit.Id, EOrderStatus.Delivered.Id)
                    },
        [UserRole.User.Name] = new HashSet<(int, int)>
                    {
                        (EOrderStatus.Pending.Id, EOrderStatus.Cancelled.Id),
                    },
        [UserRole.Admin.Name] = new HashSet<(int, int)>
                    {
                        (EOrderStatus.Pending.Id, EOrderStatus.Accepted.Id),
                        (EOrderStatus.Pending.Id,    EOrderStatus.Cancelled.Id),
                        (EOrderStatus.Accepted.Id,   EOrderStatus.Preparing.Id),
                        (EOrderStatus.Preparing.Id,  EOrderStatus.ReadyForPickup.Id),
                        (EOrderStatus.ReadyForPickup.Id,  EOrderStatus.InTransit.Id),
                        (EOrderStatus.InTransit.Id,  EOrderStatus.Delivered.Id),
                    },
    };

    public static bool CanTransition(string role, EOrderStatus from, EOrderStatus to)
    {
        return _transitions.TryGetValue(role, out HashSet<(int From, int To)>? allowed) &&
               allowed.Contains((from.Id, to.Id));
    }

    public static IEnumerable<EOrderStatus> AllowedNext(string role, EOrderStatus from)
    {
        if (!_transitions.TryGetValue(role, out HashSet<(int From, int To)>? allowed))
        {
            return Enumerable.Empty<EOrderStatus>();
        }

        return allowed
            .Where(t => t.From == from.Id)
            .Select(t => EOrderStatus.FromValue(t.To)!)
            .Where(x => x != null);
    }
}
