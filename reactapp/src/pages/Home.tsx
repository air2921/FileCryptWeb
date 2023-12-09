import React from 'react';

function Home() {
    return (
        <div className="main-container">
            <div className="head-container">
                <div className="description">
                    <p>Key Features:</p>
                    <ul>
                        <li>Robust Security: AES256 encryption ensures that your files are protected with a strong cryptographic algorithm</li>
                        <li>User-Friendly Interface: FileCryptWeb offers a simple and intuitive interface, making it easy for users to encrypt their files without technical expertise</li>
                        <li>Secure Key Exchange: FileCryptWeb provides unique functionality for secure exchange of encryption keys between users<br />
                            This provides a security guarantee that your keys will not be compromised, since you do not transfer the keys through third-party services</li>
                        <li>Fast and Efficient: The encryption process is optimized for speed, allowing you to secure your files quickly and efficiently</li>
                        <li>Security of your data: We do not share your data with anyone. Your data is also protected as we encrypt and hash your data for an additional layer of security</li>
                    </ul>
                </div>
            </div>
            <div className="body-container">
                <div className="advantages">
                    <h2 className="why-we-are">Why you should choose FileCryptWeb</h2>
                    <div className="first-arg">
                        <p>We do not store any data associated with your identity</p>
                    </div>
                    <div className="second-arg">
                        We do not analyze your files for any content, do not save them, and do not transfer them to third-party services
                    </div>
                    <div className="third-arg">
                        You can safely store your encryption keys in our service. You won't have to store them in other places where they could be compromised
                    </div>
                </div>
                <p>Protect your files today with FileCryptWeb and experience the peace of mind that comes with state-of-the-art encryption technology</p>
            </div>
            <div>
                <a href="/auth/signup">Try it</a> absolutely for free
            </div>
            <div>
                The source code of the project is open and available on <a href="https://github.com/air2921/FileCryptWeb-ASP-React">GitHub</a>
            </div>
        </div>
    );
}

export default Home;