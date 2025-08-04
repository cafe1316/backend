import db from "../db";
import { cafe1316Cart } from "../../drizzle/schema";
import { and,eq } from "drizzle-orm";

interface addCartProductParams{
    userId: string;
    productId: string;
    amount: number;
}
export const addCartProduct = async ({userId, productId, amount}: addCartProductParams): Promise<void> => {
    if(!userId || !productId || !amount){
        throw new Error("userId, productId, and amount are required");
    }
    const existing = await db.select().from(cafe1316Cart).where(and(
        eq(cafe1316Cart.userId, userId), 
        eq(cafe1316Cart.productId, productId)));

    if(existing.length > 0) {
        await db.update(cafe1316Cart).set({
            amount: amount,
            updatedAt: new Date().toISOString(),
        }).where(eq(cafe1316Cart.id, existing[0].id));
    }else{
        await db.insert(cafe1316Cart).values({
            userId,
            productId,
            amount,
            createdAt: new Date().toISOString(),
            updatedAt: new Date().toISOString(),
        });
    }
}