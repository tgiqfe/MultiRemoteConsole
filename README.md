# MultiRemoteConsole

mrc [/F <設定ファイル>][/S <接続先URI>][/A <接続許可URI>][/X <実行プロセス>]

オプション：  
    /F  設定ファイルへのパス  
    /S  クライアントモード。接続先サーバのURI。例) ws://192.168.1.10:3000  
    /A  サーバモード。接続許可するクライアントのURI。例) http://*:3000  
    /X  実行するプロセス。  
        2019年2月現在、cmdとpowershellのみ対応。bash等はそのうち対応予定  

---

WindowsでもMacでもLinuxでも、.NETやMonoがあればリモートコンソールしたい。<br>
過去に作った似たようなものを、WebSocket版にして作り直したものです。<br>