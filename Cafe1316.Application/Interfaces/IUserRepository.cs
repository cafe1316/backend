using Cafe1316.Domain.Entities;

namespace Cafe1316.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default); //通过 Email 查找用户（检查是否已存在
    Task<User> AddAsync(User user); //创建新用户
    Task UpdateAsync(User user); //更新用户信息（如头像）
}