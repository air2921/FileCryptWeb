import React from 'react';
import Message from '../../Message/Message';
import Font from '../../Font/Font';
import DateComponent from '../../Helpers/Date/Date';

function OfferList({ offers, user_id, isOwner, deleteOffer, acceptOffer }: OfferListProps) {

    if (!offers || offers.every(offer => offer === null)) {
        return <div><Message message={'No sended or received offers'} font={'storage'} /></div>;
    }

    return (
        <ul>
            <Message message={'Your Offers'} font='storage' />
            {offers
                .filter(offer => offer !== null)
                .map(offer => (
                    <li key={offer.offer_id} className="offer">
                        <div className="offer_header">
                            <div>{offer.offer_type} trade #{offer.offer_id}</div>
                            {offer.is_accepted ? (
                                <Font font={'check_small'} />
                            ) : (
                                <Font font={'close_small'} />
                            )}
                        </div>
                        <div className="offer-details">
                            <div className="time">{<DateComponent date={offer.created_at} />}</div>
                            <div className="brief-offer-info">
                                <div className="sender">Sender#{offer.sender_id}</div>
                                <div className="receiver">Receiver#{offer.receiver_id}</div>
                            </div>
                        </div>
                        {!offer.is_accepted && user_id === offer.receiver_id && acceptOffer && (
                            <button onClick={() => acceptOffer(offer.offer_id)}>Accept</button>
                        )}
                        {isOwner && deleteOffer && (
                            <button onClick={() => deleteOffer(offer.offer_id)}>
                                <Font font={'delete'} />
                            </button>
                        )}
                    </li>
                ))}
        </ul>
    );
}

export default OfferList;