import React, { useState } from 'react';
import Board from '../../../components/activityBoard/Board';
import { ActivityProps, DayActivityProps } from '../../../utils/api/Activity';

function Home() {
    const [act, setAct] = useState<DayActivityProps[] | null>(new Array(length = 0));

    function set() {
        const arr: DayActivityProps[] = new Array();
        arr.push({ Date: '01.01.2024', ActivityCount: 25, Activities: setAddAct(25) });
        arr.push({ Date: '03.02.2024', ActivityCount: 42, Activities: setAddAct(42) });
        arr.push({ Date: '15.03.2024', ActivityCount: 17, Activities: setAddAct(17) });
        arr.push({ Date: '27.04.2024', ActivityCount: 38, Activities: setAddAct(38) });
        arr.push({ Date: '02.05.2024', ActivityCount: 56, Activities: setAddAct(56) });
        arr.push({ Date: '04.05.2024', ActivityCount: 12, Activities: setAddAct(12) });
        arr.push({ Date: '07.04.2024', ActivityCount: 31, Activities: setAddAct(31) });
        arr.push({ Date: '10.03.2024', ActivityCount: 49, Activities: setAddAct(49) });
        arr.push({ Date: '13.02.2024', ActivityCount: 23, Activities: setAddAct(23) });
        arr.push({ Date: '16.01.2024', ActivityCount: 45, Activities: setAddAct(45) });
        arr.push({ Date: '19.04.2024', ActivityCount: 27, Activities: setAddAct(27) });
        arr.push({ Date: '21.03.2024', ActivityCount: 39, Activities: setAddAct(39) });
        arr.push({ Date: '24.02.2024', ActivityCount: 18, Activities: setAddAct(18) });
        arr.push({ Date: '26.01.2024', ActivityCount: 36, Activities: setAddAct(36) });
        arr.push({ Date: '29.04.2024', ActivityCount: 50, Activities: setAddAct(50) });
        arr.push({ Date: '30.03.2024', ActivityCount: 63, Activities: setAddAct(63) });
        arr.push({ Date: '01.02.2024', ActivityCount: 21, Activities: setAddAct(21) });
        arr.push({ Date: '03.01.2024', ActivityCount: 47, Activities: setAddAct(47) });
        arr.push({ Date: '05.04.2024', ActivityCount: 33, Activities: setAddAct(33) });
        arr.push({ Date: '07.03.2024', ActivityCount: 44, Activities: setAddAct(44) });

        setAct(arr);
    }

    function getRandomNumber(): number {
        const numbers = [101, 102, 201, 202, 301, 302, 303];
        const randomIndex = Math.floor(Math.random() * numbers.length);
        return numbers[randomIndex];
    }

    function setAddAct(count: number) {
        const arr: ActivityProps[] = new Array();

        for (let i = 0; i < count; i++) {
            arr.push({ action_date: '', action_id: 1, user_id: 1, type_id: getRandomNumber() })
        }

        return arr;
    }

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
                <Board days={act!} year={2024} />
                <button onClick={() => set()}>Set</button>
            </div>
        </div>
    );
}

export default Home;