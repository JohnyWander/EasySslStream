# EasySslStream
Library for client/Server ssl connections.
Uses .Net's SslStream for ssl connections, and openssl for generating certificates.

Main functionality is to handle sending text messages, sending files, directiories, byte buffers, without forcing user to handle Streams
It also support generating certificates needed for connection - using openssl.

The project itself is my way to learn .net and it evolves in proportion to my knowledge.
It also serves me as a tool in IT administrator work.

# Features 
<h3>Certificate generation using openssl - </h2>
<ul>
<li>Endusers certificates</li>
<li>Server certificates</li>
<li>CA certificates</li>
<li>CSR (Certificate signing request) generation</li>
<li>Signing CSR files</li>
<li>Converting signed CSR to optionnally password protected PKCS#12 (.pfx) </li>

</ul>

<h3>Connections</h3>
<ul>
  <li>Client <--> Server connections</li>
  <li>Async / sync support</li>
  <li>Possibility to monitor transfer rate</li>  
  <li>Multiple clients handling by single server</li>
  <li>Sending raw bytes buffers </li>
  <li>Sending text messages</li>
  <li>Sending files</li>
  <li>Sending directories</li>
</ul>

<h3>Other</h3>
<ul>
<li>For now only windows supported</li>
<li>*Linux support coming soon*</li>
</ul>
