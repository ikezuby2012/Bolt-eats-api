using System;
using System.Collections.Generic;
using System.Text;
using Application.Abstractions.Interface;

namespace Application.Abstractions.Data;

public interface IUnitOfWork
{
    void Save();
    Task SaveAsync(CancellationToken cancellationToken);
    IUserRepository UserRepository { get; }
}
