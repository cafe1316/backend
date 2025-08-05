import type{Request, Response} from "express"
import { addCartProduct, getCartItems, updateCartItem, deleteCartItem } from "../services/cartService";

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

export const updateCartController = async (req: Request, res: Response) => {
    try {
        const cartItemId = parseInt(req.params.id);
        if (isNaN(cartItemId)){
            res.status(400).json({status:400, msg: "Invalid cart item ID"});
            return;
        }

        const {amount} = req.body;
        if (!amount || typeof amount !== "number" || amount<1){
            res.status(400).json({status:400, msg:"Amount is required and must be a number and at least 1"});
            return;
        }

        const userId = req.user?.userId;
        if(!userId){
            res.status(401).json({ status: 401, msg: "Unauthorized: missing user token" });
            return;
        }
        await updateCartItem(cartItemId, userId, amount);

        res.status(200).json({ status: 200, msg: "Cart item updated successfully" });
        return;

    }catch(error:any){
        res.status(500).json({ status: 500, msg: error.message });
        return;
    }
}

export const deleteCartController = async (req: Request, res: Response) => {
    try {
        const cartItemId = parseInt(req.params.id);
        if (isNaN(cartItemId)){
            res.status(400).json({status:400, msg:"Invalid cart item ID"});
            return;
        }

        const userId = req.user?.userId;
        if(!userId){
            res.status(401).json({status:401, msg:"Unauthorized: missing user token"});
            return;
        }
        await deleteCartItem(cartItemId, userId);

        res.status(200).json({status:200, msg:"Cart item deleted successfully"});
        return;
    }catch(error:any){
        res.status(500).json({ status: 500, msg: error.message });
        return;
    }
}
