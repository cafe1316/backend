import type{Request, Response} from "express"
import { addCartProduct } from "../services/cartService";

export const addCartController = async (req: Request, res: Response) => {
    try{
        const { productId, amount } = req.body;
        if (!productId || !amount) {
            res.status(400).json({status:400 , msg:"productId and amount are required"});
            return;
        }

        const user = req.user;
        if (!user?.userId) {
        return res.status(401).json({ status: 401, msg: "Unauthorized: missing user token" });
        }

        await addCartProduct({
            userId: user.userId,
            productId,
            amount,
        });
    }catch(error: any){
        return res.status(500).json({ status: 500, msg: error.message });
    }
}