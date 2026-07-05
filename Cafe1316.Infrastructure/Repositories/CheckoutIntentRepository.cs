using Cafe1316.Application.Interfaces;
using Cafe1316.Domain.Entities;
using Cafe1316.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cafe1316.Infrastructure.Repositories;

public class CheckoutIntentRepository : ICheckoutIntentRepository
{
    //这是 EF Core 的"数据库连接器"。
    //通过它我们可以访问 _context.CheckoutIntents、_context.Orders 等所有表。
    public readonly ApplicationDbContext _context;

    public CheckoutIntentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutIntent> AddAsync(CheckoutIntent checkoutIntent, CancellationToken cancellationToken = default)
    {
        //把实体标记为"待插入"状态（此时还没真正写入数据库）。
        //为什么是 Async? 数据库操作是 I/O 密集型，用异步可以释放线程处理其他请求。
        await _context.CheckoutIntents.AddAsync(checkoutIntent, cancellationToken);

        //关键！ 只有调用这个方法，EF Core 才会真正生成 SQL 语句并发送给数据库。
        //它会执行类似：INSERT INTO checkout_intents (uuid, user_id, ...) VALUES (...)
        await _context.SaveChangesAsync(cancellationToken);
        return checkoutIntent;
    }

    public async Task<CheckoutIntent?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        return await _context.CheckoutIntents
            //Eager Loading (预加载)。
            //如果不写这行，当你访问 checkoutIntent.User.Email 时会报错（User 是 null）。
            //加了这行，EF Core 会自动 JOIN users 表，一次性把数据取回来。
            //SQL 等价：SELECT * FROM checkout_intents LEFT JOIN users ON ...
            .Include(c => c.User)

            //查找第一条 Uuid 匹配的记录。
            //如果找不到，返回 null（这就是为什么返回类型是 CheckoutIntent?）。
            .FirstOrDefaultAsync(c => c.Uuid == uuid, cancellationToken);
    }

    public async Task<CheckoutIntent?> GetByUuidForUpdateAsync(
        Guid uuid,
        CancellationToken cancellationToken = default)
    {
        var checkoutIntent = await _context.CheckoutIntents
            .FromSqlInterpolated($$"""
                SELECT * FROM checkout_intents
                WHERE "Uuid" = {{uuid}}
                FOR UPDATE
                """)
            .SingleOrDefaultAsync(cancellationToken);

        if (checkoutIntent != null)
        {
            await _context.Entry(checkoutIntent)
                .Reference(c => c.User)
                .LoadAsync(cancellationToken);
        }

        return checkoutIntent;
    }

    public async Task UpdateAsync(CheckoutIntent checkoutIntent, CancellationToken cancellationToken = default)
    {
        //告诉 EF Core："这个实体被修改了，你去生成 UPDATE 语句吧。"
        //SQL 等价：UPDATE checkout_intents SET completed_order_id = ... WHERE id = ...
        _context.CheckoutIntents.Update(checkoutIntent);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
