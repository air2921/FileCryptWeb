import React from 'react';
import Error from '../Error/Error';
import DateComponent from '../Date/Date';

function OfferList({ offers, isOwner }: OfferListProps) {

    if (!offers || offers.every(offer => offer === null)) {
        return <div><Error errorMessage={'No sended or received offers'} errorFont={'home_storage'} /></div>;
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
                                <span className="material-icons-sharp">check_small</span>
                            ) : (
                                <span className="material-icons-sharp">close_small</span>
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