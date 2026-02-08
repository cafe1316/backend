using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现
/// 负责用户数据的增删改查
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user); //把 User 对象添加到 EF Core 的追踪中 此时还没写入数据库！ 只是标记为 "待添加"
        await _context.SaveChangesAsync(); //生成 SQL INSERT 语句 此时才真正写入数据库
        return user; //返回用户对象 此时 user.Id 已经有值了！（数据库自动生成）
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user); // ← 标记为已修改
        await _context.SaveChangesAsync(); // ← 保存到数据库
    }
}