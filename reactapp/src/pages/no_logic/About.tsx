import React from 'react';

function About() {
    return (
        <div className="about">
            <div className="head-container">
                <h1>About Us</h1>
                <h3>Welcome to FileCryptWeb. Your reliable partner in securing your files !</h3>
                <div className="mission-head">
                    <h2>Our Mission</h2>
                    <div className="mission-body">
                        We aim to provide you with a highly efficient and confidential way to encrypt files of any type using the powerful AES algorithm.
                        At FileCryptWeb, we emphasize complete anonymity and the security of your data, ensuring that your content remains solely yours.
                    </div>
                </div>
            </div>
            <div className="body-container">
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
                        <li>
                            <h4>Support for All Popular MIME Types</h4>
                            <div>
                                We guarantee full compatibility by supporting all popular MIME types.
                                This ensures universality and flexibility in using our service.
                            </div>
                        </li>
                    </ul>
                </div>
                <div className="additionally-container">
                    <div className="commitment-container">
                        <div className="commitment-head">
                            <h2>Our Commitment</h2>
                            <div className="commitment-body">
                                While the service was created with boundless enthusiasm,
                                it doesn't hinder us from ensuring the maximum security of your data and the high performance of our service.
                                Our team of experienced security professionals continually works on enhancing encryption mechanisms to provide you with reliable protection.
                            </div>
                        </div>
                    </div>
                    <div className="file-size-limit-container">
                        <div className="limit-head">
                            <h2>File Size Limitation</h2>
                            <div className="limit-body">
                                For your convenience, we allow file encryption up to 75 MB in size.
                                This enables you to easily maintain confidentiality and security while preserving the efficiency of using our service.
                            </div>
                        </div>
                    </div>
                    <div className="free-container">
                        <div className="free-head">
                            <h2>Free and Ad-Free</h2>
                            <div className="free-body">
                                FileCryptWeb takes pride in being completely free for all users.
                                We are confident in the quality of our services and do not charge for their use. Additionally, we value your time and comfort, so we do not interrupt your experience with advertisements.
                                Our goal is to offer you a simple, effective, and secure tool without additional inconveniences.
                            </div>
                        </div>
                    </div>
                </div>
                <div className="join-container">
                    <h2>Join FileCryptWeb</h2>
                    <div>
                        Join FileCryptWeb today to benefit from secure and convenient file encryption.
                        We strive to make your online experience as safe and efficient as possible, while maintaining simplicity and free access.
                        <h3>Your security is our priority</h3>
                    </div>
                </div>
                <p>You can read our <a href="/policy">Policy</a></p>
            </div>
        </div>
    );
}

export default About;