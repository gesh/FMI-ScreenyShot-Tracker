<?php

namespace Piss;

use Doctrine\DBAL\Tools\Console\Helper\ConnectionHelper;
use Doctrine\ORM\Tools\Console\ConsoleRunner;
use Doctrine\ORM\Tools\Console\Helper\EntityManagerHelper;
use Symfony\Component\Console\Helper\HelperSet;

/**
 * Description of CliHelper
 *
 * @author x0r
 */
class CliHelper {

    public static function startCli($em) {

        $helperSet = new HelperSet(array(
            'db' => new ConnectionHelper($em->getConnection()),
            'em' => new EntityManagerHelper($em)
        ));
        $metadatas = $em->getMetadataFactory()->getAllMetadata();
        $repositoryName = $em->getConfiguration()->getDefaultRepositoryClassName();
//dump_r($metadatas,false,false);
//dump_r($repositoryName,false,false);

        $commands = array();
        ConsoleRunner::run($helperSet, $commands);
    }

}
