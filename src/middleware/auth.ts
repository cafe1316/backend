import type {Request, Response, NextFunction} from "express";
import jwt from "jsonwebtoken";

const secret: string = process.env.JWT_SECRET || "";

export const authMiddleware = async (req: Request, res: Response, next:NextFunction) => {
    const authHeader = req.headers.authorization;
    if (!authHeader) {
        res.send({status:403, msg:"Token required!"})
        return;    
    }
    const token = authHeader.split(" ")[1];
    try{
        jwt.verify(token, secret) as { userId: string; email?: string };
        // req.user = {
        // userId: decoded.userId,
        // email: decoded.email,
        // };
        next();
    }catch(error){
        res.status(403).json({ message: "Unauthorized: Invalid or expired token" });
        return;
    }   
}