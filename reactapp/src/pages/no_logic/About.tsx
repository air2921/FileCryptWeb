import React from 'react';

function About() {
    return (
        <div className="about">
            <h1>About Us</h1>
            <div className="container">
                <div className="features">
                    <ul>
                        <li>
                            <h4>Encryption without Tracking</h4>
                            <div>
                                We ensure the full anonymity of your files, guaranteeing that your personal information remains just that personal.
                                No data about you or your content is shared with third parties.
                            </div>
                        </li>
                        <li>
                            <h4>Centralized Key Storage</h4>
                            <div>
                                We offer a convenient centralized repository for your encryption keys.
                                You no longer need to trust third-party services.
                                All key operations can be performed directly on our website.
                            </div>
                        </li>
                        <li>
                            <h4>Secure Key Exchange</h4>
                            <div>
                                We provide a secure way to exchange encryption keys with other users through our key exchange system.
                                This adds an extra layer of confidentiality to your data.
                            </div>
                        </li>
                        <li>
                            <h4>Encryption of Any Content</h4>
                            <div>
                                FileCryptWeb supports the encryption of files with any content, except potentially malicious content.
                                We provide a reliable way to protect your information.
                            </div>
                        </li>
                    </ul>
                </div>
                <div className="additionally-container">
                    <h2>Our Commitment</h2>
                    <div className="commitment-body">
                        While the service was created with boundless enthusiasm,
                        it doesn't hinder us from ensuring the maximum security of your data and the high performance of our service.
                        Our team of experienced security professionals continually works on enhancing encryption mechanisms to provide you with reliable protection.
                    </div>
                    <h2>File Size Limitation</h2>
                    <div className="limit-body">
                        For your convenience, we allow file encryption up to 75 MB in size.
                        This enables you to easily maintain confidentiality and security while preserving the efficiency of using our service.
                    </div>
                    <h2>Free and Ad-Free</h2>
                    <div className="free-body">
                        FileCryptWeb takes pride in being completely free for all users.
                        We are confident in the quality of our services and do not charge for their use. Additionally, we value your time and comfort, so we do not interrupt your experience with advertisements.
                        Our goal is to offer you a simple, effective, and secure tool without additional inconveniences.
                    </div>
                </div>
                <div className="join-container">
                    <h2>Join FileCryptWeb</h2>
                    <div>
                        <h3>Your security is our priority</h3>
                        Join FileCryptWeb today to benefit from secure and convenient file encryption.
                        We strive to make your online experience as safe and efficient as possible, while maintaining simplicity and free access.
                    </div>
                </div>
                <p>You can read our <a href="/policy">policy</a></p>
            </div>
        </div>
    );
}

export default About;