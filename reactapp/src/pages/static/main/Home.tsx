import React, { useState } from 'react';
import Board, { ActivityProps } from '../../../components/activityBoard/Board';

function Home() {
    return (
        <div className="home">
            <div className="head-container">
                <div className="description">
                    <h2>Key Features:</h2>
                    <ul>
                        <li>Robust Security: Utilize AES256 encryption to ensure your files are protected with a strong cryptographic algorithm.</li>
                        <li>User-Friendly Interface: FileCryptWeb offers a simple and intuitive interface, enabling users to encrypt files without requiring technical expertise.</li>
                        <li>Secure Key Exchange: Enjoy the unique functionality of secure encryption key exchange between users on FileCryptWeb. This ensures that your keys remain uncompromised, as they are not transferred through third-party services.</li>
                        <li>Fast and Efficient: Optimize the encryption process for speed, allowing you to secure your files quickly and efficiently.</li>
                        <li>Data Security Guarantee: We do not share your data with anyone. Your data is further protected through encryption and hashing for an additional layer of security.</li>
                    </ul>
                </div>
            </div>
            <div className="body-container">
                <div className="advantages">
                    <h2 className="why-we-are">Why Choose FileCryptWeb</h2>
                    <div className="first-arg">
                        <p>We never store any data associated with your identity.</p>
                    </div>
                    <div className="second-arg">
                        We neither analyze your files for content, not save them, not transfer them to third-party services.
                    </div>
                    <div className="third-arg">
                        Safely store your encryption keys in our service, eliminating the need to store them in vulnerable places where they could be compromised.
                    </div>
                </div>
                <p>Secure your files today with FileCryptWeb and experience the peace of mind that comes with state-of-the-art encryption technology.</p>
            </div>
            <div>
                <a href="/auth/signup">Try it</a> absolutely for free.
            </div>
            <div>
                The source code of the project is open and available on <a href="https://github.com/air2921/FileCryptWeb-ASP-React">GitHub</a>.
            </div>
            <div>
                <Board days={new Array(length = 0)} year={2022} />
            </div>
        </div>
    );
}

export default Home;