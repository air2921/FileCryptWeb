import React from 'react';
import DateComponent from '../Date/Date';
import Message from '../Message/Message';

function OfferList({ offers, isOwner }: OfferListProps) {

    if (!offers || offers.every(offer => offer === null)) {
        return <div><Message message={'No sended or received offers'} font={'home_storage'} /></div>;
    }

    return (
        <ul>
            {offers
                .filter(offer => offer !== null)
                .map(offer => (
                    <li key={offer.offer_id} className="offer">
                        <div className="offer_header">
                            <span>{offer.offer_type} trade #{offer.offer_id}</span>
                            {offer.is_accepted ? (
                                <i className="material-icons-sharp">check_small</i>
                            ) : (
                                <i className="material-icons-sharp">close_small</i>
                            )}
                        </div>
                        <div className="offer-details">
                            <div className="time">{<DateComponent date={offer.created_at} />}</div>
                            <div className="brief-file-info">
                                <div className="sender">Sender#{offer.sender_id}</div>
                                <div className="receiver">Receiver#{offer.receiver_id}</div>
                            </div>
                        </div>
                        {isOwner && <button>Delete</button>}
                    </li>
                ))}
        </ul>
    );
}

export default OfferList;