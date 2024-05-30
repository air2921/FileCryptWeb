import React from "react";
import { OfferProps } from "../../../utils/api/Offers";
import { dateFormate } from "../../../utils/helpers/Formatter";

function Offer({ offer, isOwner }: { offer: OfferProps, isOwner: boolean }) {
    function setTradeType(type: number): string {
        if (type = 101) {
            return 'Key Offer'
        } else {
            return 'Unknown'
        }
    }

    return (
        <div className="entity-offer-container">
            <div className="entity-offer-type">Type: {setTradeType(offer.offer_type)}</div>
            <div className="entity-offer-status">{offer.is_accepted ? "Already accepted" : "Pending acceptance"}</div>
            <div className="entity-offer-participants">
                <div className="entity-offer-info">
                    #{offer.sender_id} opened a offer#{offer.offer_id} to #{offer.receiver_id} at: {dateFormate(offer.created_at)}
                </div>
                {isOwner && offer.offer_body && offer.offer_header && (
                    <div className="entity-offer-main">
                        <div className="entity-offer-header">Header: {offer.offer_header}</div>
                        <div className="entity-offer-body">Body: {offer.offer_body}</div>
                    </div>
                )};
            </div>
        </div>
    );
}

export default Offer;