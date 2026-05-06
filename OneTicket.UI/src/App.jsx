import React, { useState, useEffect } from 'react';
import api, { buyTicket, requestSinglePayout, uploadBatchFile } from './services/api';
import { Ticket, CreditCard, Clock, Send, Layers, Loader2, Zap, PartyPopper, FileSpreadsheet, ExternalLink } from 'lucide-react';

export default function App() {
    const [view, setView] = useState('tickets');
    const [paymentInfo, setPaymentInfo] = useState(null);
    const [purchaseStatus, setPurchaseStatus] = useState('idle');
    const [timeLeft, setTimeLeft] = useState(900);

    const [loadingTicket, setLoadingTicket] = useState(false);
    const [loadingSingle, setLoadingSingle] = useState(false);
    const [loadingBatch, setLoadingBatch] = useState(false);
    const [selectedFile, setSelectedFile] = useState(null);

    const [payoutForm, setPayoutForm] = useState({
        amount: 2500,
        beneficiaryName: "Garry Payout",
        accountNumber: "3333888800",
        connectorId: 221300,
        description: "OneTicket Withdrawal"
    });

    const isProcessingPayout = loadingSingle || loadingBatch;

    // 1. POLLING LOGIC (Checks if Webhook has updated status to 'Paid')
    useEffect(() => {
        let poll;
        if (purchaseStatus === 'pending' && paymentInfo?.reference) {
            poll = setInterval(async () => {
                try {
                    const res = await api.get(`/Tickets/status/${paymentInfo.reference}`);
                    if (res.data.status === 'Paid') {
                        setPurchaseStatus('success');
                        clearInterval(poll);
                    }
                } catch (e) { console.log("Polling for payment status..."); }
            }, 3000);
        }
        return () => clearInterval(poll);
    }, [purchaseStatus, paymentInfo]);

    // 2. TICKET PURCHASE (Redirects to Hosted Checkout)
    const handleBuy = async () => {
        setLoadingTicket(true);
        try {
            const res = await buyTicket("demo", "balalagarry@gmail.com");

            if (res.data.checkoutUrl) {
                setPaymentInfo(res.data);
                setPurchaseStatus('pending');

                // REDIRECT to OneKhusa Hosted Checkout Page
                window.location.href = res.data.checkoutUrl;
            } else {
                alert("Error: No Checkout URL received from server.");
            }
        } catch (e) {
            console.error("Initiation Failed:", e.response?.data);
            const reason = e.response?.data?.details || "Server unreachable or Invalid API Keys";
            alert(`❌ Initiation Failed: ${reason}`);
        } finally {
            setLoadingTicket(false);
        }
    };

    // 3. SINGLE PAYOUT
    const handleSinglePayout = async () => {
        setLoadingSingle(true);
        try {
            const res = await requestSinglePayout({
                ...payoutForm,
                userId: "cef9ee62-b4fa-4488-857f-9547e0681c9c",
                email: "balalagarry@gmail.com"
            });
            alert(`✅ SUCCESS!\nOneKhusa Ref: ${res.data.refNum}`);
        } catch (err) {
            alert("❌ Payout Failed: Check Portal Balance or API Credentials");
        } finally {
            setLoadingSingle(false);
        }
    };

    // 4. BATCH PAYOUT
    const handleFileUpload = async () => {
        if (!selectedFile) return alert("Please select a file.");
        setLoadingBatch(true);
        try {
            const res = await uploadBatchFile(selectedFile, 'balalagarry@gmail.com');
            alert(`🚀 BATCH UPLOADED!\nBatch No: ${res.data.batchNumber}`);
            setSelectedFile(null);
        } catch (err) {
            const errorMsg = err.response?.data?.details || "Check file format";
            alert(`❌ File Upload Failed: ${errorMsg}`);
        } finally {
            setLoadingBatch(false);
        }
    };

    return (
        <div className="min-h-screen bg-slate-50 p-4 md:p-8 font-sans text-slate-900">
            <header className="max-w-7xl mx-auto flex flex-col md:flex-row justify-between items-center mb-10">
                <h1 className="text-3xl font-black text-blue-700 uppercase tracking-tighter">ONETICKET <span className="text-slate-300 font-light italic">FINTECH</span></h1>
                <div className="flex bg-white p-1.5 rounded-2xl border shadow-sm mt-4 md:mt-0">
                    <button onClick={() => setView('tickets')} className={`px-8 py-2 rounded-xl font-bold text-xs transition ${view === 'tickets' ? 'bg-blue-600 text-white shadow-md' : 'text-slate-400'}`}>COLLECTIONS</button>
                    <button onClick={() => setView('payouts')} className={`px-8 py-2 rounded-xl font-bold text-xs transition ${view === 'payouts' ? 'bg-blue-600 text-white shadow-md' : 'text-slate-400'}`}>DISBURSEMENTS</button>
                </div>
            </header>

            <main className="max-w-7xl mx-auto">
                {purchaseStatus === 'success' ? (
                    <div className="bg-white p-12 rounded-[3rem] border shadow-2xl text-center animate-in zoom-in duration-500">
                        <PartyPopper size={80} className="mx-auto text-emerald-500 mb-6" />
                        <h2 className="text-4xl font-black mb-4">Payment Verified!</h2>
                        <button onClick={() => setPurchaseStatus('idle')} className="bg-blue-600 text-white px-12 py-4 rounded-[2rem] font-black uppercase tracking-widest">Buy Another</button>
                    </div>
                ) : (
                    <>
                        {view === 'tickets' ? (
                            <div className="max-w-2xl mx-auto space-y-6">
                                <div className="bg-white p-10 rounded-[3rem] border shadow-sm text-center">
                                    <Ticket className="mx-auto text-blue-600 mb-4 opacity-20" size={60} />
                                    <h2 className="text-2xl font-bold mb-2">Showcase Entry</h2>
                                    <p className="text-4xl font-black mb-10">MWK 2,500</p>

                                    <button
                                        onClick={handleBuy}
                                        disabled={loadingTicket || purchaseStatus === 'pending'}
                                        className="w-full bg-slate-900 text-white py-5 rounded-[2rem] font-black text-lg flex justify-center items-center gap-3 disabled:opacity-50 transition hover:bg-blue-600"
                                    >
                                        {loadingTicket ? <Loader2 className="animate-spin" /> : <ExternalLink />}
                                        {purchaseStatus === 'pending' ? "PAYMENT IN PROGRESS..." : "SECURE CHECKOUT"}
                                    </button>
                                    <p className="mt-4 text-xs text-slate-400 uppercase font-bold tracking-widest">Redirects to OneKhusa Hosted Page</p>
                                </div>

                                {purchaseStatus === 'pending' && (
                                    <div className="text-center animate-pulse">
                                        <p className="font-bold text-blue-600">Waiting for you to complete payment on the checkout page...</p>
                                    </div>
                                )}
                            </div>
                        ) : (
                            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8 animate-in fade-in duration-300">
                                <div className="bg-white p-10 rounded-[3rem] border shadow-sm space-y-6">
                                    <h2 className="text-xl font-black flex items-center gap-3 uppercase"><Zap className="text-amber-500 fill-amber-500" /> Instant Withdrawal</h2>
                                    <div className="space-y-4">
                                        <input type="number" value={payoutForm.amount} onChange={e => setPayoutForm({ ...payoutForm, amount: e.target.value })} className="w-full border-2 border-slate-100 p-4 rounded-2xl bg-slate-50 font-bold" placeholder="Amount" />
                                        <input type="text" value={payoutForm.accountNumber} onChange={e => setPayoutForm({ ...payoutForm, accountNumber: e.target.value })} className="w-full border-2 border-slate-100 p-4 rounded-2xl bg-slate-50 font-bold" placeholder="Account Number" />
                                        <button onClick={handleSinglePayout} disabled={isProcessingPayout} className="w-full bg-blue-600 text-white py-5 rounded-3xl font-black flex justify-center items-center gap-3 hover:bg-blue-700 transition disabled:opacity-50">
                                            {loadingSingle ? <Loader2 className="animate-spin" /> : <Send size={20} />}
                                            {loadingSingle ? "SENDING..." : "DISBURSE NOW"}
                                        </button>
                                    </div>
                                </div>

                                <div className="bg-slate-900 text-white p-10 rounded-[3rem] shadow-xl space-y-6 border border-slate-800">
                                    <h2 className="text-xl font-black flex items-center gap-3 uppercase"><Layers className="text-blue-400" /> Batch Disbursements</h2>
                                    <div className="p-8 border-2 border-dashed border-slate-700 rounded-[2.5rem] text-center bg-white/5">
                                        <FileSpreadsheet className="mx-auto text-slate-500 mb-4" size={48} />
                                        <input type="file" id="batchFile" hidden accept=".xlsx, .csv" onChange={(e) => setSelectedFile(e.target.files[0])} />
                                        <label htmlFor="batchFile" className="cursor-pointer bg-white text-slate-900 px-6 py-2 rounded-xl font-bold text-xs hover:bg-slate-100 mb-6 inline-block">
                                            {selectedFile ? selectedFile.name : "CHOOSE FILE"}
                                        </label>
                                        <button onClick={handleFileUpload} disabled={!selectedFile || isProcessingPayout} className="w-full bg-blue-500 text-white py-5 rounded-3xl font-black flex justify-center items-center gap-3 hover:bg-blue-400 transition disabled:opacity-50">
                                            {loadingBatch ? <Loader2 className="animate-spin" /> : <Layers size={20} />}
                                            {loadingBatch ? "UPLOADING..." : "EXECUTE BATCH"}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        )}
                    </>
                )}
            </main>
        </div>
    );
}