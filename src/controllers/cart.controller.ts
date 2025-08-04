import type{Request, Response} from "express"
import { addCartProduct, getCartItems } from "../services/cartService";

export const addCartController = async (req: Request, res: Response) => {
    try{
        const { productId, amount } = req.body;
        if (!productId || !amount) {
            res.status(400).json({status:400 , msg:"productId and amount are required"});
            return;
        }

        const user = req.user;
        if (!user?.userId) {
            res.status(401).json({ status: 401, msg: "Unauthorized: missing user token" });
            return;
        }

        await addCartProduct({
            userId: user.userId,
            productId,
            amount,
        });
        res.status(200).json({ status: 200, msg: "Product added to cart successfully" });
        return;
        
    }catch(error: any){
        res.status(500).json({ status: 500, msg: error.message });
        return;
    }
}

export const getCartController = async (req: Request, res: Response) => {
    try {
        const user = req.user;
        if (!user?.userId) {
            res.status(401).json({status:401, msg:"Unauthorized: missing user token"});
            return;
        }

        const cartItems = await getCartItems(user.userId);
        res.status(200).json({status:200, data:cartItems});
        return;
    }catch (error:any) {
        res.status(500).json({ status: 500, msg: error.message });
        return;
    }
}